using System;
using System.Linq;
using Xunit;

namespace GtkSharp.Tests
{
    /// <summary>
    /// The four graphene calls that pass N boxed structs laid end to end —
    /// <c>Rect.GetVertices</c>, <c>Box.GetVertices</c>, <c>Frustum.GetPlanes</c>
    /// and <c>Quad.InitFromPoints</c>.
    /// </summary>
    /// <remarks>
    /// This is the one array shape GapiCodegen cannot express. Every graphene
    /// type is bound as an opaque <em>class</em>, so a <c>Graphene.Vec2[]</c>
    /// marshals as an array of pointers, and what these C functions want is four
    /// <c>graphene_vec2_t</c> structures back to back. The whole file
    /// (<c>FixedVertexArrays.cs</c>) exists to bridge that by hand, and nothing
    /// referenced it by name.
    ///
    /// The oracle is arithmetic the test does itself: a rectangle's corners
    /// follow from its origin and size, and a box's eight corners are the
    /// combinations of its two extremes. Nothing here trusts graphene to tell the
    /// test what the answer should be.
    ///
    /// Reading *every* element matters more than usual. A wrapper that pointed at
    /// the start of the buffer would give the right first element and garbage
    /// afterwards, which is exactly how this shape has failed elsewhere in this
    /// repository.
    /// </remarks>
    public class GrapheneVertexArrayTests : GtkTestBase
    {
        public GrapheneVertexArrayTests(GtkFixture fixture) : base(fixture) { }

        static Graphene.Rect Rect(float x, float y, float w, float h)
        {
            var rect = Graphene.Rect.Alloc();
            rect.Init(x, y, w, h);
            return rect;
        }

        static Graphene.Point Point(float x, float y)
        {
            var point = new Graphene.Point();
            point.Init(x, y);
            return point;
        }

        static Graphene.Point3D Point3D(float x, float y, float z)
        {
            var point = new Graphene.Point3D();
            point.Init(x, y, z);
            return point;
        }

        static (float X, float Y) Xy(Graphene.Vec2 v) => (v.X, v.Y);
        static (float X, float Y, float Z) Xyz(Graphene.Vec3 v) => (v.X, v.Y, v.Z);

        // ------------------------------------------------------------ Rect

        [Fact]
        public void A_rectangle_reports_its_four_corners_in_the_documented_order()
        {
            // graphene documents top-left, top-right, bottom-right, bottom-left,
            // and every one of the four is checked -- a buffer read at the wrong
            // stride gives the first corner correctly and then drifts.
            Run(() =>
            {
                var vertices = Rect(10, 20, 30, 40).GetVertices();

                Assert.Equal(4, vertices.Length);

                Assert.Equal((10f, 20f), Xy(vertices[0]));    // top-left
                Assert.Equal((40f, 20f), Xy(vertices[1]));    // top-right
                Assert.Equal((40f, 60f), Xy(vertices[2]));    // bottom-right
                Assert.Equal((10f, 60f), Xy(vertices[3]));    // bottom-left
            });
        }

        [Fact]
        public void A_rectangle_at_the_origin_has_corners_that_are_not_all_the_same()
        {
            // The control for reading at the wrong stride: with a zero origin,
            // three of the four corners would still look plausible if every
            // element came from the same address, so the distinctness is the
            // assertion rather than the values.
            Run(() =>
            {
                var vertices = Rect(0, 0, 2, 3).GetVertices();

                Assert.Equal(4, vertices.Select(Xy).Distinct().Count());
                Assert.Equal((0f, 0f), Xy(vertices[0]));
                Assert.Equal((2f, 3f), Xy(vertices[2]));
            });
        }

        [Fact]
        public void Each_returned_vertex_is_its_own_allocation()
        {
            // Split copies every element into its own block precisely so that no
            // wrapper points into the middle of somebody else's. Two vertices
            // sharing storage would show up as one changing when the other is
            // written.
            Run(() =>
            {
                var vertices = Rect(0, 0, 10, 10).GetVertices();

                Assert.NotEqual(vertices[0].Handle, vertices[1].Handle);
                Assert.NotEqual(vertices[1].Handle, vertices[2].Handle);

                vertices[0].Init(99, 99);

                Assert.Equal((99f, 99f), Xy(vertices[0]));
                Assert.Equal((10f, 0f), Xy(vertices[1]));    // untouched
            });
        }

        [Fact]
        public void The_vertices_outlive_the_rectangle_they_came_from()
        {
            // The buffer is freed inside GetVertices, and the wrappers own their
            // own copies -- so they have to stay readable after everything that
            // produced them is gone. Reading through a freed buffer would be
            // undefined rather than merely wrong.
            Run(() =>
            {
                Graphene.Vec2[] vertices;

                {
                    var rect = Rect(1, 2, 3, 4);
                    vertices = rect.GetVertices();
                    rect.Dispose();
                }

                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();

                Assert.Equal((1f, 2f), Xy(vertices[0]));
                Assert.Equal((4f, 6f), Xy(vertices[2]));
            });
        }

        // ------------------------------------------------------------- Box

        [Fact]
        public void A_box_reports_eight_distinct_corners_spanning_its_extremes()
        {
            // The eight corners are every combination of the two extremes, which
            // the test can enumerate itself without knowing graphene's ordering.
            Run(() =>
            {
                var box = new Graphene.Box();
                box.Init(Point3D(0, 0, 0), Point3D(1, 2, 3));

                var vertices = box.GetVertices();

                Assert.Equal(8, vertices.Length);

                var corners = vertices.Select(Xyz).ToHashSet();

                Assert.Equal(8, corners.Count);

                foreach (var x in new[] { 0f, 1f })
                    foreach (var y in new[] { 0f, 2f })
                        foreach (var z in new[] { 0f, 3f })
                            Assert.Contains((x, y, z), corners);
            });
        }

        [Fact]
        public void Every_corner_of_a_box_is_read_rather_than_only_the_first()
        {
            // A stride of zero would give eight copies of one corner, and a
            // stride that is wrong by a few bytes would give a first corner that
            // is right and seven that are nonsense. Counting the distinct ones
            // catches both.
            Run(() =>
            {
                var box = new Graphene.Box();
                box.Init(Point3D(-5, -5, -5), Point3D(5, 5, 5));

                var corners = box.GetVertices().Select(Xyz).ToHashSet();

                Assert.Equal(8, corners.Count);
                Assert.All(corners, c =>
                {
                    Assert.True(c.Item1 == -5f || c.Item1 == 5f, $"x was {c.Item1}");
                    Assert.True(c.Item2 == -5f || c.Item2 == 5f, $"y was {c.Item2}");
                    Assert.True(c.Item3 == -5f || c.Item3 == 5f, $"z was {c.Item3}");
                });
            });
        }

        // ------------------------------------------------------------ Quad

        [Fact]
        public void A_quad_hands_back_the_four_points_it_was_built_from()
        {
            // The other direction: Pack copies four wrappers into one contiguous
            // buffer. A round trip through GetPoint is the whole contract, and
            // every index is checked because packing at the wrong stride writes
            // the later points over each other.
            Run(() =>
            {
                var points = new[]
                {
                    Point(0, 0),
                    Point(10, 0),
                    Point(10, 5),
                    Point(0, 5),
                };

                var quad = new Graphene.Quad();
                quad.InitFromPoints(points);

                Assert.Equal(0f, quad.GetPoint(0).X, 3);
                Assert.Equal(10f, quad.GetPoint(1).X, 3);
                Assert.Equal(5f, quad.GetPoint(2).Y, 3);
                Assert.Equal(0f, quad.GetPoint(3).X, 3);
                Assert.Equal(5f, quad.GetPoint(3).Y, 3);
            });
        }

        [Fact]
        public void A_quad_built_from_points_covers_the_area_they_enclose()
        {
            // An oracle outside the round trip: graphene decides for itself what
            // the quad contains, and the answer has to agree with the rectangle
            // the test drew.
            Run(() =>
            {
                var quad = new Graphene.Quad();
                quad.InitFromPoints(new[] { Point(0, 0), Point(10, 0), Point(10, 5), Point(0, 5) });

                Assert.True(quad.Contains(Point(5, 2)), "the middle is inside");
                Assert.False(quad.Contains(Point(50, 2)), "well outside is not");

                var bounds = quad.Bounds();
                Assert.Equal(10f, bounds.Width, 3);
                Assert.Equal(5f, bounds.Height, 3);
            });
        }

        [Fact]
        public void A_quad_refuses_the_wrong_number_of_points()
        {
            // Pack cannot tell how long the C function expects the buffer to be,
            // so the count is checked in managed code. Passing three would
            // otherwise read a fourth point from past the end of the buffer.
            Run(() =>
            {
                var quad = new Graphene.Quad();

                Assert.Throws<ArgumentException>(
                    () => quad.InitFromPoints(new[] { Point(0, 0), Point(1, 0), Point(1, 1) }));

                Assert.Throws<ArgumentException>(
                    () => quad.InitFromPoints(new Graphene.Point[0]));

                Assert.Throws<ArgumentException>(() => quad.InitFromPoints(null));
            });
        }

        [Fact]
        public void A_quad_refuses_a_null_point_rather_than_packing_a_null_handle()
        {
            Run(() =>
            {
                var quad = new Graphene.Quad();

                Assert.Throws<ArgumentNullException>(
                    () => quad.InitFromPoints(new[] { Point(0, 0), null, Point(1, 1), Point(0, 1) }));
            });
        }

        // --------------------------------------------------------- Frustum

        [Fact]
        public void A_frustum_reports_six_planes_that_are_not_all_the_same()
        {
            Run(() =>
            {
                // A frustum from an identity matrix is the canonical clip volume,
                // which is enough to have six real planes.
                var frustum = new Graphene.Frustum();
                var identity = new Graphene.Matrix();
                identity.InitIdentity();
                frustum.InitFromMatrix(identity);

                var planes = frustum.GetPlanes();

                Assert.Equal(6, planes.Length);
                Assert.All(planes, p => Assert.NotEqual(IntPtr.Zero, p.Handle));

                // Six separate allocations, not six views of one.
                Assert.Equal(6, planes.Select(p => p.Handle).Distinct().Count());
            });
        }

        [Fact]
        public void A_frustums_planes_describe_the_volume_it_says_it_contains()
        {
            // The planes are only worth anything if they agree with the frustum's
            // own containment test, which is computed independently of them.
            Run(() =>
            {
                var frustum = new Graphene.Frustum();
                var identity = new Graphene.Matrix();
                identity.InitIdentity();
                frustum.InitFromMatrix(identity);

                var planes = frustum.GetPlanes();
                var inside = Point3D(0, 0, 0);

                Assert.True(frustum.ContainsPoint(inside), "the origin is inside the clip volume");

                // Every plane must agree: a point inside is on the non-negative
                // side of all six.
                foreach (var plane in planes)
                    Assert.True(plane.Distance(inside) >= -0.001f,
                                $"the origin should not be behind a clipping plane, was {plane.Distance(inside)}");

                Assert.False(frustum.ContainsPoint(Point3D(0, 0, 100)), "far away is not");
            });
        }
    }
}
