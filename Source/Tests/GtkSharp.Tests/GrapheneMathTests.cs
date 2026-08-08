using System;
using System.Linq;
using Xunit;

namespace GtkSharp.Tests
{
    /// <summary>
    /// Graphene, and the arithmetic half of Gsk.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is the corner of the binding with the best oracles in the
    /// repository, because the answers are arithmetic that can be done here: a
    /// matrix times its inverse is the identity, a 3-4-5 triangle has area 6, a
    /// ray fired down the x axis at a box spanning [-1, 1] enters it at t = 4,
    /// the planes of a 60-degree frustum meet the axis at 30 degrees. Nothing
    /// below asks graphene what the answer is and then agrees with it.
    /// </para>
    /// <para>
    /// <b>Tolerances.</b> Graphene computes in <c>float</c> and, on any build
    /// with SIMD, through <c>__m128</c>, so a 24-bit mantissa is the floor:
    /// about seven significant decimal digits, and less once a value has been
    /// through a trigonometric call or a matrix decomposition. Assertions here
    /// use four decimal places on quantities of order 1 to 100, which is two to
    /// three digits of headroom over the observed error and still far tighter
    /// than any real defect. Where a result is exact in binary floating point --
    /// a translation read straight back out, a slerp endpoint, a halving -- it
    /// is asserted exactly, and where graphene is *not* exact when it looks like
    /// it should be, that is the subject of the test rather than a reason to
    /// loosen it. See <see cref="Interpolating_a_matrix_is_not_exact_even_at_the_endpoints"/>.
    /// </para>
    /// <para>
    /// Most of what is exercised here could not be called at all until the
    /// codegen fixes that came with this file: graphene's girs spell a
    /// predicate's return type <c>c:type="bool"</c> rather than
    /// <c>gboolean</c>, which the symbol table did not know, so every one of the
    /// 49 methods returning one -- <c>Inverse</c>, <c>Decompose</c>,
    /// <c>ContainsPoint</c>, <c>Intersection</c>, every <c>Equal</c> and
    /// <c>Near</c> -- was silently dropped.
    /// </para>
    /// </remarks>
    public class GrapheneMathTests : GtkTestBase
    {
        public GrapheneMathTests(GtkFixture fixture) : base(fixture) { }

        // ------------------------------------------------------------- helpers

        private static Graphene.Point Pt(float x, float y)
        {
            var p = new Graphene.Point();
            p.Init(x, y);
            return p;
        }

        private static Graphene.Point3D Pt3(float x, float y, float z)
        {
            var p = new Graphene.Point3D();
            p.Init(x, y, z);
            return p;
        }

        private static Graphene.Vec3 V3(float x, float y, float z)
        {
            var v = new Graphene.Vec3();
            v.Init(x, y, z);
            return v;
        }

        private static Graphene.Rect RectAt(float x, float y, float width, float height)
        {
            var r = Graphene.Rect.Alloc();
            r.Init(x, y, width, height);
            return r;
        }

        private static Graphene.Matrix Identity()
        {
            var m = new Graphene.Matrix();
            m.InitIdentity();
            return m;
        }

        private static Graphene.Matrix Translation(float x, float y, float z)
        {
            var m = new Graphene.Matrix();
            m.InitTranslate(Pt3(x, y, z));
            return m;
        }

        private static Graphene.Matrix Scaling(float x, float y, float z)
        {
            var m = new Graphene.Matrix();
            m.InitScale(x, y, z);
            return m;
        }

        private static Graphene.Matrix RotationZ(float degrees)
        {
            var m = new Graphene.Matrix();
            m.InitRotate(degrees, V3(0, 0, 1));
            return m;
        }

        /// <summary>Every element of a matrix, row by row.</summary>
        private static float[] Elements(Graphene.Matrix m)
        {
            var result = new float[16];
            for (uint row = 0; row < 4; row++)
                for (uint col = 0; col < 4; col++)
                    result[row * 4 + col] = m.GetValue(row, col);
            return result;
        }

        // ================================================================ Matrix

        [Fact]
        public void A_matrix_times_its_inverse_is_the_identity()
        {
            // graphene_matrix_inverse returns bool, which is why it did not exist
            // in this binding until now. The oracle is the definition of an
            // inverse; asserted both ways round, because M*inv and inv*M being
            // equal is the other half of what "inverse" means.
            Run(() =>
            {
                var m = Scaling(2, 4, 8).Multiply(RotationZ(37)).Multiply(Translation(5, -6, 7));

                Graphene.Matrix inverse;
                Assert.True(m.Inverse(out inverse), "an invertible matrix reported no inverse");

                foreach (var product in new[] { m.Multiply(inverse), inverse.Multiply(m) })
                {
                    var elements = Elements(product);
                    for (int row = 0; row < 4; row++)
                        for (int col = 0; col < 4; col++)
                            Assert.Equal(row == col ? 1f : 0f, elements[row * 4 + col], 4);
                }
            });
        }

        [Fact]
        public void A_singular_matrix_has_no_inverse_and_says_so()
        {
            // A scale by zero collapses three dimensions to a point, so its
            // determinant is 0*0*0 and nothing can undo it. This is the branch
            // that separates "inverse" from "the identity came back by accident".
            Run(() =>
            {
                var flattened = Scaling(2, 3, 0);
                Assert.Equal(0f, flattened.Determinant(), 6);
                Assert.True(flattened.IsSingular);

                Graphene.Matrix inverse;
                Assert.False(flattened.Inverse(out inverse), "a singular matrix reported an inverse");

                // ... while a matrix that merely looks unusual is fine.
                Assert.False(Scaling(2, 3, 4).IsSingular);
                Assert.Equal(24f, Math.Abs(Scaling(2, 3, 4).Determinant()), 4);
            });
        }

        [Fact]
        public void The_determinant_graphene_reports_has_the_opposite_sign()
        {
            // Not a binding defect -- graphene_matrix_determinant returns a plain
            // float and there is no marshalling in the way -- but worth pinning,
            // because the magnitude is right and only the sign is wrong, so it
            // passes every test that squares it or compares it against zero.
            //
            // The identity's determinant is 1 by definition. Graphene says -1.
            // Every consequence follows from that one sign: a scale by (2,3,4)
            // reports -24, and a mirroring transform -- which is the one thing
            // the sign of a determinant is actually used for, since a negative
            // determinant means the handedness was flipped -- reports +24. So a
            // renderer deciding whether to reverse its winding order from
            // `determinant < 0` gets the answer backwards on every matrix.
            //
            // IsSingular is unaffected, because zero has no sign.
            Run(() =>
            {
                Assert.Equal(-1f, Identity().Determinant(), 5);
                Assert.Equal(-1f, Translation(5, 6, 7).Determinant(), 5);
                Assert.Equal(-1f, RotationZ(37).Determinant(), 5);

                Assert.Equal(-24f, Scaling(2, 3, 4).Determinant(), 4);
                Assert.Equal(24f, Scaling(-2, 3, 4).Determinant(), 4);
            });
        }

        [Fact]
        public void Multiply_applies_the_receiver_first_not_the_argument()
        {
            // The trap. graphene_matrix_multiply(a, b, res) is documented as
            // "multiplies a by b", which reads like the mathematical product AB
            // and is the opposite way round from what happens to a point: the
            // RECEIVER is applied first. Getting this backwards produces a
            // transform that is wrong only when the operations do not commute,
            // which a scale and a translate do not.
            //
            // Both answers below are computed here: scale-then-translate takes
            // (1,1,1) to (2,2,2) and then to (12,22,32); translate-then-scale
            // takes it to (11,21,31) and then to (22,42,62).
            Run(() =>
            {
                var scale = Scaling(2, 2, 2);
                var translate = Translation(10, 20, 30);

                var scaleThenTranslate = scale.Multiply(translate).TransformPoint3d(Pt3(1, 1, 1));
                Assert.Equal(12f, scaleThenTranslate.X, 4);
                Assert.Equal(22f, scaleThenTranslate.Y, 4);
                Assert.Equal(32f, scaleThenTranslate.Z, 4);

                var translateThenScale = translate.Multiply(scale).TransformPoint3d(Pt3(1, 1, 1));
                Assert.Equal(22f, translateThenScale.X, 4);
                Assert.Equal(42f, translateThenScale.Y, 4);
                Assert.Equal(62f, translateThenScale.Z, 4);
            });
        }

        [Fact]
        public void A_matrix_reads_out_row_major_with_the_translation_in_the_last_row()
        {
            // graphene_matrix_to_float fills a float[16] the CALLER supplies. The
            // parameter came out of codegen as a single float, so the binding
            // passed one value in a vector register and let graphene write
            // sixty-four bytes through whatever the pointer register happened to
            // hold. Reading a matrix's elements out -- the most basic thing there
            // is to do with one -- could not be done.
            //
            // The layout is the oracle: graphene is row-vector, so a translation
            // lives in the LAST ROW, and to_float agrees with get_value(row, col).
            Run(() =>
            {
                var m = Translation(10, 20, 30);
                var v = m.ToFloat();

                Assert.Equal(16, v.Length);
                Assert.Equal(new float[]
                {
                    1, 0, 0, 0,
                    0, 1, 0, 0,
                    0, 0, 1, 0,
                    10, 20, 30, 1,
                }, v);

                Assert.Equal(v, Elements(m));
                Assert.Equal(10f, m.GetValue(3, 0), 4);
                Assert.Equal(0f, m.GetValue(0, 3), 4);
                Assert.Equal(10f, m.XTranslation, 4);
            });
        }

        [Fact]
        public void A_matrix_built_from_sixteen_floats_is_the_one_they_describe()
        {
            // The input half of the same fix. The round trip alone would pass on
            // a binding that ignored the array entirely and returned the
            // receiver, so the matrix is also asked to transform a point: the
            // sixteen floats below are a scale by (2,3,4) plus a translation of
            // (5,6,7), which takes (1,1,1) to (7,9,11).
            Run(() =>
            {
                var v = new float[]
                {
                    2, 0, 0, 0,
                    0, 3, 0, 0,
                    0, 0, 4, 0,
                    5, 6, 7, 1,
                };

                var m = new Graphene.Matrix();
                m.InitFromFloat(v);

                Assert.Equal(v, m.ToFloat());

                var moved = m.TransformPoint3d(Pt3(1, 1, 1));
                Assert.Equal(7f, moved.X, 4);
                Assert.Equal(9f, moved.Y, 4);
                Assert.Equal(11f, moved.Z, 4);
            });
        }

        [Fact]
        public void A_fixed_size_array_argument_of_the_wrong_length_is_refused()
        {
            // There is no count argument, so C cannot check this and graphene
            // reads sixteen floats whatever it is given: a fifteen-element array
            // is a read past the end of the managed block with nothing to catch
            // it. The generated guard is the only thing between a caller and
            // that, so it is worth a test of its own.
            Run(() =>
            {
                var m = new Graphene.Matrix();

                Assert.Throws<ArgumentException>(() => m.InitFromFloat(new float[15]));
                Assert.Throws<ArgumentException>(() => m.InitFromFloat(new float[17]));
                Assert.Throws<ArgumentException>(() => m.InitFromFloat(null));

                // Three-element rows for a triangle, not one float each.
                var triangle = new Graphene.Triangle();
                Assert.Throws<ArgumentException>(
                    () => triangle.InitFromFloat(new float[3], new float[2], new float[3]));
            });
        }

        [Fact]
        public void Transposing_swaps_the_indices_and_is_its_own_inverse()
        {
            Run(() =>
            {
                var m = new Graphene.Matrix();
                m.InitFrom2d(1, 2, 3, 4, 5, 6);

                var t = m.Transpose();
                for (uint row = 0; row < 4; row++)
                    for (uint col = 0; col < 4; col++)
                        Assert.Equal(m.GetValue(row, col), t.GetValue(col, row), 6);

                Assert.True(m.Equal(t.Transpose()), "transposing twice did not give the original back");
            });
        }

        [Fact]
        public void Decompose_recovers_the_translation_scale_and_rotation_it_was_built_from()
        {
            // Five caller-allocated out parameters, one of them a quaternion, on
            // a method that returns bool -- so it did not exist here either.
            Run(() =>
            {
                var m = Scaling(2, 3, 4).Multiply(Translation(5, 6, 7));

                Graphene.Vec3 translate, scale, shear;
                Graphene.Quaternion rotate;
                Graphene.Vec4 perspective;
                Assert.True(m.Decompose(out translate, out scale, out rotate, out shear, out perspective));

                Assert.Equal(new float[] { 5, 6, 7 }, translate.ToFloat().Select(f => (float)Math.Round(f, 4)).ToArray());
                Assert.Equal(new float[] { 2, 3, 4 }, scale.ToFloat().Select(f => (float)Math.Round(f, 4)).ToArray());

                // No shear, no perspective, and the rotation is the identity
                // quaternion (0,0,0,1) -- which is what makes this a real
                // assertion rather than five values read back unexamined.
                foreach (var s in shear.ToFloat())
                    Assert.Equal(0f, s, 4);
                Assert.Equal(new float[] { 0, 0, 0, 1 }, perspective.ToFloat().Select(f => (float)Math.Round(f, 3)).ToArray());
                Assert.Equal(new float[] { 0, 0, 0, 1 }, rotate.ToVec4().ToFloat().Select(f => (float)Math.Round(f, 3)).ToArray());
            });
        }

        [Fact]
        public void Interpolating_a_matrix_is_not_exact_even_at_the_endpoints()
        {
            // graphene_matrix_interpolate does not blend the sixteen elements:
            // it decomposes both matrices into translation, scale, shear,
            // perspective and a quaternion, blends those, and multiplies a fresh
            // matrix back out. So factor 0 need not hand back the first matrix
            // bit for bit -- under gvsbuild it comes back carrying about 2.4e-4
            // of rotation that was never there.
            //
            // "Need not", and that is as far as this goes. An earlier version
            // asserted the round trip was NOT exact, and CI proved that wrong:
            // on the runner's vector unit the decomposition recomposes exactly
            // and the assertion failed. Whether a floating-point operation is
            // exact is a property of the hardware and the compiler, never of
            // the library, so it is not something a test may require in either
            // direction. What IS true on every build, and what an animation
            // actually depends on, is that the translation moves linearly.
            //
            // One decimal place: seven times the largest recomposition error
            // seen (6.9e-3 on z under Debian's build) and still fifty times
            // tighter than the five-unit gap between consecutive samples.
            Run(() =>
            {
                var from = Translation(0, 0, 0);
                var to = Translation(10, 20, 30);

                foreach (var pair in new[]
                {
                    Tuple.Create(0.0, 0f), Tuple.Create(0.5, 5f), Tuple.Create(1.0, 10f),
                })
                {
                    var mid = from.Interpolate(to, pair.Item1);
                    Assert.Equal(pair.Item2, mid.XTranslation, 1);
                    Assert.Equal(pair.Item2 * 2, mid.YTranslation, 1);
                    Assert.Equal(pair.Item2 * 3, mid.ZTranslation, 1);
                }
            });
        }

        [Fact]
        public void Transformed_bounds_are_the_extents_of_the_transformed_corners()
        {
            // graphene_matrix_transform_bounds must produce the axis-aligned box
            // around the four transformed corners, and the four corners can be
            // transformed here one at a time -- so the oracle is the library's
            // own point transform, used four times, against its bounds transform
            // used once. A unit square turned 45 degrees becomes a diamond of
            // side 10, whose extent is 10*sqrt(2) on both axes.
            Run(() =>
            {
                var rotate = RotationZ(45);
                var rect = RectAt(0, 0, 10, 10);

                var corners = new[] { Pt(0, 0), Pt(10, 0), Pt(10, 10), Pt(0, 10) }
                    .Select(rotate.TransformPoint).ToArray();

                var bounds = rotate.TransformBounds(rect);

                Assert.Equal(corners.Min(p => p.X), bounds.X, 4);
                Assert.Equal(corners.Min(p => p.Y), bounds.Y, 4);
                Assert.Equal(corners.Max(p => p.X) - corners.Min(p => p.X), bounds.Width, 4);
                Assert.Equal(corners.Max(p => p.Y) - corners.Min(p => p.Y), bounds.Height, 4);

                Assert.Equal(10f * (float)Math.Sqrt(2), bounds.Width, 4);
            });
        }

        [Fact]
        public void Is2d_and_IsIdentity_describe_the_matrix_rather_than_how_it_was_made()
        {
            Run(() =>
            {
                Assert.True(Identity().IsIdentity);
                Assert.True(Identity().Is2d());

                // A z translation is not two-dimensional even though nothing
                // about the call that made it mentions a third dimension.
                Assert.False(Translation(1, 2, 3).Is2d());
                Assert.True(Translation(1, 2, 0).Is2d());

                // IsIdentity is an EXACT comparison, so it is not a question to
                // ask of a matrix that has been through any arithmetic. A scale
                // times its own inverse is the identity to the last bit on
                // Windows and a few ulp away from it on Debian -- same graphene,
                // different compiler and vector unit -- so neither answer is a
                // fact about the binding. Near is the one that means something
                // here, and 1e-6 is four orders of magnitude tighter than the
                // difference any real defect would make.
                var roundTrip = Scaling(4, 4, 4);
                Graphene.Matrix inverse;
                Assert.True(roundTrip.Inverse(out inverse));
                Assert.True(roundTrip.Multiply(inverse).Near(Identity(), 1e-6f));

                // What IsIdentity does answer reliably is a matrix built to be
                // something else, however small the difference.
                Assert.False(Scaling(1, 1, 1.0001f).IsIdentity);
                Assert.True(Scaling(1, 1, 1).IsIdentity);
            });
        }

        // =============================================================== Vectors

        [Fact]
        public void The_cross_product_is_perpendicular_to_both_and_anticommutative()
        {
            // Two identities that hold for every pair of vectors, so neither can
            // be satisfied by reading a value back.
            Run(() =>
            {
                var a = V3(1, 2, 3);
                var b = V3(-4, 5, 6);

                var cross = a.Cross(b);
                Assert.Equal(0f, cross.Dot(a), 4);
                Assert.Equal(0f, cross.Dot(b), 4);

                var reversed = b.Cross(a);
                Assert.Equal(cross.ToFloat(), reversed.Negate().ToFloat());

                // And the right-handed basis, which fixes the sign convention.
                Assert.Equal(new float[] { 0, 0, 1 }, V3(1, 0, 0).Cross(V3(0, 1, 0)).ToFloat());
            });
        }

        [Fact]
        public void Normalising_keeps_the_direction_and_makes_the_length_one()
        {
            // 3-4-5, so the length is exact and the normalised components are
            // 0.6 and 0.8 -- neither of which is exactly representable in binary,
            // hence four places rather than an exact comparison.
            Run(() =>
            {
                var v = V3(3, 4, 0);
                Assert.Equal(5f, v.Length(), 4);

                var unit = v.Normalize();
                Assert.Equal(1f, unit.Length(), 4);
                Assert.Equal(0.6f, unit.X, 4);
                Assert.Equal(0.8f, unit.Y, 4);

                // Same direction: the cross product with the original vanishes
                // and the dot product is the original's length.
                Assert.Equal(0f, unit.Cross(v).Length(), 4);
                Assert.Equal(5f, unit.Dot(v), 4);
            });
        }

        [Fact]
        public void Near_is_the_comparison_with_a_tolerance_and_Equal_is_not()
        {
            // Both are new -- they return bool. The pair matters because Equal is
            // an exact float comparison, so a value that has been through any
            // arithmetic at all will fail it, and the caller who reaches for the
            // obvious name gets a silent false.
            Run(() =>
            {
                var a = V3(1, 0, 0);
                var b = V3(1.0001f, 0, 0);

                Assert.False(a.Equal(b));
                Assert.True(a.Near(b, 0.001f));
                Assert.False(a.Near(b, 0.00001f));

                Assert.True(a.Equal(V3(1, 0, 0)));

                // Matrices have the same pair, with the same meaning.
                var identity = Identity();
                var nudged = Identity();
                nudged.Translate(Pt3(0.001f, 0, 0));
                Assert.False(identity.Equal(nudged));
                Assert.True(identity.Near(nudged, 0.01f));
                Assert.False(identity.Near(nudged, 0.0001f));
            });
        }

        // =========================================================== Rectangles

        [Fact]
        public void Intersecting_rectangles_gives_the_overlap_and_disjoint_ones_say_so()
        {
            Run(() =>
            {
                Graphene.Rect overlap;
                Assert.True(RectAt(0, 0, 10, 10).Intersection(RectAt(5, 5, 10, 10), out overlap));
                Assert.Equal(5f, overlap.X, 4);
                Assert.Equal(5f, overlap.Y, 4);
                Assert.Equal(5f, overlap.Width, 4);
                Assert.Equal(5f, overlap.Height, 4);

                // On failure the out parameter is NOT null -- it is a zeroed
                // rectangle, which is a perfectly ordinary value. So the bool is
                // the only thing that separates "no overlap" from "an empty
                // rectangle at the origin", and a caller who checks the result
                // for null concludes that everything intersects.
                Graphene.Rect none;
                Assert.False(RectAt(0, 0, 10, 10).Intersection(RectAt(50, 50, 10, 10), out none));
                Assert.NotNull(none);
                Assert.Equal(0f, none.Width, 4);
                Assert.Equal(0f, none.Height, 4);
            });
        }

        [Fact]
        public void Inset_changes_the_rectangle_it_is_called_on_and_InsetR_does_not()
        {
            // graphene's in-place operations are spelled without a suffix and the
            // copying ones with _r, which is the reverse of what a C# caller
            // expects from a method that returns a value. `var smaller =
            // rect.Inset (1, 1);` compiles, reads like a pure function, and
            // quietly shrinks the rectangle the caller still holds. Offset and
            // Normalize are the same shape.
            Run(() =>
            {
                var mutated = RectAt(0, 0, 10, 10);
                var returned = mutated.Inset(1, 2);
                Assert.Equal(1f, mutated.X, 4);
                Assert.Equal(2f, mutated.Y, 4);
                Assert.Equal(8f, mutated.Width, 4);
                Assert.Equal(6f, mutated.Height, 4);
                Assert.Equal(mutated.Width, returned.Width, 4);

                var kept = RectAt(0, 0, 10, 10);
                var copy = kept.InsetR(1, 2);
                Assert.Equal(0f, kept.X, 4);
                Assert.Equal(10f, kept.Width, 4);
                Assert.Equal(1f, copy.X, 4);
                Assert.Equal(8f, copy.Width, 4);

                var moved = RectAt(0, 0, 10, 10);
                moved.Offset(5, 5);
                Assert.Equal(5f, moved.X, 4);
                var stillThere = RectAt(0, 0, 10, 10);
                stillThere.OffsetR(5, 5);
                Assert.Equal(0f, stillThere.X, 4);
            });
        }

        [Fact]
        public void A_rectangle_contains_every_point_on_its_boundary()
        {
            // Inclusive on all four edges, which means two rectangles laid side
            // by side BOTH contain the point where they meet. Hit-testing a
            // stack of adjacent regions by asking each one in turn therefore has
            // no unique answer on a boundary, and the order of the walk decides
            // it.
            Run(() =>
            {
                var left = RectAt(0, 0, 10, 10);
                var right = RectAt(10, 0, 10, 10);
                var shared = Pt(10, 5);

                Assert.True(left.ContainsPoint(shared));
                Assert.True(right.ContainsPoint(shared));

                Assert.True(left.ContainsPoint(Pt(0, 0)));
                Assert.True(left.ContainsPoint(Pt(10, 10)));
                Assert.False(left.ContainsPoint(Pt(10.001f, 5)));

                Assert.True(left.ContainsRect(RectAt(1, 1, 2, 2)));
                Assert.False(left.ContainsRect(RectAt(1, 1, 20, 2)));
            });
        }

        [Fact]
        public void Union_covers_both_rectangles_and_interpolation_walks_between_them()
        {
            Run(() =>
            {
                var union = RectAt(0, 0, 10, 10).Union(RectAt(20, 20, 10, 10));
                Assert.Equal(0f, union.X, 4);
                Assert.Equal(30f, union.Width, 4);
                Assert.Equal(30f, union.Height, 4);
                Assert.True(union.ContainsRect(RectAt(0, 0, 10, 10)));
                Assert.True(union.ContainsRect(RectAt(20, 20, 10, 10)));

                // Unlike a matrix, a rectangle interpolates componentwise, so the
                // halfway point is the arithmetic mean exactly and the endpoints
                // are the endpoints.
                var half = RectAt(0, 0, 10, 10).Interpolate(RectAt(10, 10, 20, 20), 0.5);
                Assert.Equal(5f, half.X, 4);
                Assert.Equal(15f, half.Width, 4);

                var start = RectAt(0, 0, 10, 10).Interpolate(RectAt(10, 10, 20, 20), 0.0);
                Assert.Equal(0f, start.X, 4);
                Assert.Equal(10f, start.Width, 4);

                Assert.Equal(100f, RectAt(0, 0, 10, 10).Area, 4);
            });
        }

        [Fact]
        public void The_four_vertices_of_a_rectangle_are_its_corners_clockwise_from_the_top_left()
        {
            // graphene_rect_get_vertices writes four graphene_vec2_t end to end,
            // and every graphene type is bound as a class, so a Vec2[] would
            // marshal as four addresses rather than sixty-four bytes. Codegen
            // used to emit the ELEMENT type -- one Vec2 -- and let graphene write
            // all four through its handle. It is hand-written now; this is what
            // says the buffer is walked with the right stride, because getting
            // that wrong shows up in vertices two and three rather than the first.
            Run(() =>
            {
                var vertices = RectAt(1, 2, 30, 40).GetVertices();

                Assert.Equal(4, vertices.Length);
                Assert.Equal(new float[] { 1, 2 }, vertices[0].ToFloat());
                Assert.Equal(new float[] { 31, 2 }, vertices[1].ToFloat());
                Assert.Equal(new float[] { 31, 42 }, vertices[2].ToFloat());
                Assert.Equal(new float[] { 1, 42 }, vertices[3].ToFloat());
            });
        }

        // ================================================================== Quad

        [Fact]
        public void A_quad_keeps_the_four_points_it_was_built_from_and_bounds_them()
        {
            // graphene_quad_init_from_points takes graphene_point_t[4], the same
            // shape as the vertices above; a quad's whole purpose is to be the
            // four corners of a rectangle after a transform that is not
            // axis-aligned, so a bounding box built from one point is exactly the
            // failure that looks plausible.
            Run(() =>
            {
                var quad = new Graphene.Quad();
                quad.InitFromPoints(new[] { Pt(0, 0), Pt(10, 2), Pt(12, 12), Pt(2, 10) });

                Assert.Equal(new float[] { 0, 0 }, new[] { quad.GetPoint(0).X, quad.GetPoint(0).Y });
                Assert.Equal(new float[] { 10, 2 }, new[] { quad.GetPoint(1).X, quad.GetPoint(1).Y });
                Assert.Equal(new float[] { 12, 12 }, new[] { quad.GetPoint(2).X, quad.GetPoint(2).Y });
                Assert.Equal(new float[] { 2, 10 }, new[] { quad.GetPoint(3).X, quad.GetPoint(3).Y });

                var bounds = quad.Bounds();
                Assert.Equal(0f, bounds.X, 4);
                Assert.Equal(12f, bounds.Width, 4);
                Assert.Equal(12f, bounds.Height, 4);

                Assert.True(quad.Contains(Pt(6, 6)));
                // Inside the bounding box and outside the quad: the left edge
                // runs from (0,0) to (2,10), so at y = 9 it is at x = 1.8.
                Assert.True(bounds.ContainsPoint(Pt(0.5f, 9)));
                Assert.False(quad.Contains(Pt(0.5f, 9)));
            });
        }

        // ============================================================== Triangle

        [Fact]
        public void A_triangle_reports_the_area_normal_and_centroid_of_its_geometry()
        {
            // Legs of 4 and 3 in the z = 0 plane: area 6 exactly, normal +z by
            // the right-hand rule for that winding, centroid the mean of the
            // three vertices.
            Run(() =>
            {
                var triangle = new Graphene.Triangle();
                triangle.InitFromPoint3d(Pt3(0, 0, 0), Pt3(4, 0, 0), Pt3(0, 3, 0));

                Assert.Equal(6f, triangle.Area, 4);
                Assert.Equal(new float[] { 0, 0, 1 }, triangle.Normal.ToFloat().Select(f => (float)Math.Round(f, 4)).ToArray());
                Assert.Equal(4f / 3, triangle.Midpoint.X, 4);
                Assert.Equal(1f, triangle.Midpoint.Y, 4);

                Assert.True(triangle.ContainsPoint(Pt3(1, 1, 0)));
                Assert.False(triangle.ContainsPoint(Pt3(3, 3, 0)));

                // The same triangle spelled as three float[3]s -- the other half
                // of the fixed-size array fix.
                var fromFloats = new Graphene.Triangle();
                fromFloats.InitFromFloat(new float[] { 0, 0, 0 }, new float[] { 4, 0, 0 }, new float[] { 0, 3, 0 });
                Assert.True(triangle.Equal(fromFloats));
            });
        }

        [Fact]
        public void Barycentric_coordinates_name_the_third_and_second_vertices_not_the_first_two()
        {
            // A trap worth a test of its own. graphene_triangle_get_barycoords
            // hands back a Vec2, and the obvious reading -- x is a's weight, y is
            // b's -- is wrong in both places: x is the weight of C and y the
            // weight of B, with A's weight left implicit as 1 - x - y. So vertex
            // a comes back as (0,0), b as (0,1) and c as (1,0), and code that
            // reconstructs a point from them silently swaps two corners.
            Run(() =>
            {
                var triangle = new Graphene.Triangle();
                triangle.InitFromPoint3d(Pt3(0, 0, 0), Pt3(4, 0, 0), Pt3(0, 3, 0));

                Graphene.Vec2 weights;

                Assert.True(triangle.GetBarycoords(Pt3(0, 0, 0), out weights));
                Assert.Equal(new float[] { 0, 0 }, weights.ToFloat().Select(f => (float)Math.Round(f, 4)).ToArray());

                Assert.True(triangle.GetBarycoords(Pt3(4, 0, 0), out weights));
                Assert.Equal(new float[] { 0, 1 }, weights.ToFloat().Select(f => (float)Math.Round(f, 4)).ToArray());

                Assert.True(triangle.GetBarycoords(Pt3(0, 3, 0), out weights));
                Assert.Equal(new float[] { 1, 0 }, weights.ToFloat().Select(f => (float)Math.Round(f, 4)).ToArray());

                // The centroid weights all three equally, which is the one point
                // where the naming cannot mislead.
                Assert.True(triangle.GetBarycoords(Pt3(4f / 3, 1, 0), out weights));
                Assert.Equal(1f / 3, weights.X, 4);
                Assert.Equal(1f / 3, weights.Y, 4);
            });
        }

        // ============================================================ Box/Sphere

        [Fact]
        public void The_eight_vertices_of_a_box_are_every_combination_of_its_two_corners()
        {
            // graphene_box_get_vertices writes eight graphene_vec3_t end to end,
            // the widest of the fixed-size arrays here. The assertion is set
            // equality against the eight corners named by hand, so it does not
            // depend on the order graphene chose -- but it does require all eight
            // to be distinct and present, which a wrong stride cannot manage.
            Run(() =>
            {
                var box = new Graphene.Box();
                box.Init(Pt3(0, 0, 0), Pt3(2, 4, 6));

                var expected = (from x in new float[] { 0, 2 }
                                from y in new float[] { 0, 4 }
                                from z in new float[] { 0, 6 }
                                select x + "," + y + "," + z).OrderBy(s => s).ToArray();

                var actual = box.GetVertices()
                    .Select(v => v.ToFloat())
                    .Select(f => f[0] + "," + f[1] + "," + f[2])
                    .OrderBy(s => s).ToArray();

                Assert.Equal(expected, actual);

                Assert.Equal(2f, box.Width, 4);
                Assert.Equal(4f, box.Height, 4);
                Assert.Equal(6f, box.Depth, 4);
                Assert.Equal(new float[] { 1, 2, 3 }, new[] { box.Center.X, box.Center.Y, box.Center.Z });
            });
        }

        [Fact]
        public void Box_intersection_is_the_overlap_and_containment_is_inclusive()
        {
            Run(() =>
            {
                var box = new Graphene.Box();
                box.Init(Pt3(0, 0, 0), Pt3(10, 10, 10));

                var other = new Graphene.Box();
                other.Init(Pt3(5, 5, 5), Pt3(20, 20, 20));

                Graphene.Box overlap;
                Assert.True(box.Intersection(other, out overlap));
                Assert.Equal(new float[] { 5, 5, 5 }, new[] { overlap.Min.X, overlap.Min.Y, overlap.Min.Z });
                Assert.Equal(new float[] { 10, 10, 10 }, new[] { overlap.Max.X, overlap.Max.Y, overlap.Max.Z });

                var far = new Graphene.Box();
                far.Init(Pt3(50, 50, 50), Pt3(60, 60, 60));
                Graphene.Box none;
                Assert.False(box.Intersection(far, out none));

                Assert.True(box.ContainsPoint(Pt3(0, 0, 0)));
                Assert.True(box.ContainsPoint(Pt3(10, 10, 10)));
                Assert.False(box.ContainsPoint(Pt3(10.001f, 5, 5)));
                Assert.True(box.ContainsBox(overlap));
                Assert.False(box.ContainsBox(other));
            });
        }

        [Fact]
        public void A_sphere_measures_distance_from_its_surface_and_loses_its_radius_when_moved()
        {
            // Two things at once, and the second is a defect in graphene rather
            // than in this binding: graphene_sphere_translate assigns the centre
            // and never touches the radius, so what comes back out of the buffer
            // is whatever was in it. This binding zeroes caller-allocated
            // buffers, which is why the answer below is 0 rather than 1.49e15 --
            // still wrong, but the same wrong every time, which is the difference
            // between a defect a test can pin and one that looks like a flake.
            Run(() =>
            {
                var sphere = new Graphene.Sphere();
                sphere.Init(Pt3(0, 0, 0), 5);

                // Distance is to the SURFACE, so a point ten units out is five
                // units away and a point on the surface is zero.
                Assert.Equal(5f, sphere.Distance(Pt3(10, 0, 0)), 4);
                Assert.Equal(0f, sphere.Distance(Pt3(3, 4, 0)), 4);
                Assert.True(sphere.ContainsPoint(Pt3(3, 4, 0)));
                Assert.False(sphere.ContainsPoint(Pt3(3, 4.001f, 0)));

                var box = sphere.BoundingBox;
                Assert.Equal(new float[] { -5, -5, -5 }, new[] { box.Min.X, box.Min.Y, box.Min.Z });
                Assert.Equal(new float[] { 5, 5, 5 }, new[] { box.Max.X, box.Max.Y, box.Max.Z });

                var moved = sphere.Translate(Pt3(1, 1, 1));
                Assert.Equal(new float[] { 1, 1, 1 }, new[] { moved.Center.X, moved.Center.Y, moved.Center.Z });
                Assert.Equal(0f, moved.Radius);
                Assert.True(moved.IsEmpty, "a translated sphere is reported non-empty; graphene has started copying the radius");
            });
        }

        [Fact]
        public void A_planes_distance_is_signed_by_which_side_the_point_is_on()
        {
            // distance(p) = dot(normal, p) + constant, which is what makes the
            // sign meaningful -- and is the whole reason a frustum can be tested
            // against a point by asking its six planes.
            Run(() =>
            {
                var plane = new Graphene.Plane();
                plane.Init(V3(0, 1, 0), -5);          // the horizontal plane y = 5

                Assert.Equal(-5f, plane.Distance(Pt3(0, 0, 0)), 4);
                Assert.Equal(0f, plane.Distance(Pt3(100, 5, -100)), 4);
                Assert.Equal(5f, plane.Distance(Pt3(0, 10, 0)), 4);

                // Negating turns the plane over: same surface, opposite sign.
                var flipped = plane.Negate();
                Assert.Equal(5f, flipped.Distance(Pt3(0, 0, 0)), 4);
                Assert.Equal(0f, flipped.Distance(Pt3(0, 5, 0)), 4);

                // Three points define the same plane, with the normal fixed by
                // their winding.
                var fromPoints = new Graphene.Plane();
                fromPoints.InitFromPoints(Pt3(0, 5, 0), Pt3(1, 5, 0), Pt3(0, 5, -1));
                Assert.Equal(new float[] { 0, 1, 0 }, fromPoints.Normal.ToFloat().Select(f => (float)Math.Round(f, 4)).ToArray());
                Assert.Equal(-5f, fromPoints.Constant, 4);
            });
        }

        // ================================================================== Ray

        [Fact]
        public void A_ray_meets_a_box_where_the_algebra_says_it_does()
        {
            // A box spanning [-1, 1] on every axis, and a ray fired from
            // (-5, 0, 0) along +x. It reaches x = -1 after four units, so t is 4
            // and the position there is (-1, 0, 0). Every number is arithmetic
            // done here.
            Run(() =>
            {
                var box = new Graphene.Box();
                box.Init(Pt3(-1, -1, -1), Pt3(1, 1, 1));

                var ray = new Graphene.Ray();
                ray.Init(Pt3(-5, 0, 0), V3(1, 0, 0));

                float t;
                Assert.Equal(Graphene.RayIntersectionKind.Enter, ray.IntersectBox(box, out t));
                Assert.Equal(4f, t, 4);
                Assert.True(ray.IntersectsBox(box));

                var hit = ray.GetPositionAt(t);
                Assert.Equal(-1f, hit.X, 4);
                Assert.Equal(0f, hit.Y, 4);

                // Started inside, so the only crossing ahead is on the way out --
                // one unit along, and reported as Leave rather than Enter.
                var inside = new Graphene.Ray();
                inside.Init(Pt3(0, 0, 0), V3(1, 0, 0));
                Assert.Equal(Graphene.RayIntersectionKind.Leave, inside.IntersectBox(box, out t));
                Assert.Equal(1f, t, 4);

                // A ray is a half-line: the box is behind it, so there is no hit
                // at all rather than a negative t.
                var past = new Graphene.Ray();
                past.Init(Pt3(5, 0, 0), V3(1, 0, 0));
                Assert.Equal(Graphene.RayIntersectionKind.None, past.IntersectBox(box, out t));
                Assert.False(past.IntersectsBox(box));
            });
        }

        [Fact]
        public void A_ray_that_misses_reports_nothing_and_measures_the_gap()
        {
            Run(() =>
            {
                var sphere = new Graphene.Sphere();
                sphere.Init(Pt3(0, 0, 0), 2);

                var ray = new Graphene.Ray();
                ray.Init(Pt3(-5, 0, 0), V3(1, 0, 0));

                float t;
                Assert.Equal(Graphene.RayIntersectionKind.Enter, ray.IntersectSphere(sphere, out t));
                Assert.Equal(3f, t, 4);   // 5 units to the centre, less the radius

                // Three units above the axis is one unit clear of a radius-2
                // sphere, so the ray misses and the closest approach is 3 from
                // the centre.
                var over = new Graphene.Ray();
                over.Init(Pt3(-5, 3, 0), V3(1, 0, 0));
                Assert.Equal(Graphene.RayIntersectionKind.None, over.IntersectSphere(sphere, out t));
                Assert.False(over.IntersectsSphere(sphere));
                Assert.Equal(3f, over.GetDistanceToPoint(Pt3(0, 0, 0)), 4);

                var closest = over.GetClosestPointToPoint(Pt3(0, 0, 0));
                Assert.Equal(0f, closest.X, 4);
                Assert.Equal(3f, closest.Y, 4);
            });
        }

        // ============================================================== Frustum

        [Fact]
        public void The_six_planes_of_a_perspective_frustum_are_the_ones_trigonometry_gives()
        {
            // The test that says the ABI is right. graphene_frustum_get_planes
            // copies six graphene_plane_t end to end, and a graphene_plane_t is
            // { 16-byte SIMD vector; float } -- 32 bytes in C, because the vector
            // aligns to 16. The generated ABI description measured it as 20, so
            // the stride was twelve bytes short and planes two, four and six came
            // back built out of denormal floats. Four of the six looked fine,
            // which is exactly how such a thing survives.
            //
            // The oracle is the geometry. For a 60-degree vertical field of view
            // with a square aspect, the four side planes make an angle of 30
            // degrees with the view axis, so each has a z component of -sin(30)
            // = -0.5 and a transverse component of cos(30); all four pass through
            // the eye, so their constants are 0. The near and far planes are
            // perpendicular to z, and their constants are the distances that were
            // asked for.
            Run(() =>
            {
                var projection = new Graphene.Matrix();
                projection.InitPerspective(60, 1, 1, 100);

                var frustum = new Graphene.Frustum();
                frustum.InitFromMatrix(projection);

                var planes = frustum.GetPlanes();
                Assert.Equal(6, planes.Length);

                // Every plane graphene hands back is normalised. A wrong stride
                // fails this before anything else does.
                foreach (var plane in planes)
                    Assert.Equal(1f, plane.Normal.Length(), 3);

                var axial = planes.Where(p => Math.Abs(p.Normal.Z) > 0.99f).ToArray();
                var sides = planes.Where(p => Math.Abs(p.Normal.Z) <= 0.99f).ToArray();
                Assert.Equal(2, axial.Length);
                Assert.Equal(4, sides.Length);

                // Near at 1 and far at 100, with opposite normals -- and the
                // constants say which is which.
                Assert.Equal(new float[] { -1, 100 },
                             axial.Select(p => (float)Math.Round(p.Constant, 3)).OrderBy(c => c).ToArray());

                var halfFov = (float)(30 * Math.PI / 180);
                foreach (var side in sides)
                {
                    Assert.Equal(-Math.Sin(halfFov), side.Normal.Z, 3);
                    Assert.Equal(Math.Cos(halfFov),
                                 Math.Sqrt(side.Normal.X * side.Normal.X + side.Normal.Y * side.Normal.Y), 3);
                    Assert.Equal(0f, side.Constant, 3);
                }

                // And the planes agree with the frustum's own containment test:
                // ten units down the view axis is inside, ten units behind the
                // eye is not.
                Assert.True(frustum.ContainsPoint(Pt3(0, 0, -10)));
                Assert.False(frustum.ContainsPoint(Pt3(0, 0, 10)));
                Assert.False(frustum.ContainsPoint(Pt3(0, 0, -1000)));
            });
        }

        [Fact]
        public void The_abi_sizes_are_the_ones_the_c_declarations_add_up_to()
        {
            // A guard on the fix above, stated as arithmetic over the C
            // declarations rather than as numbers read out of the binding.
            // graphene_simd4f_t is a 16-byte vector that aligns to 16, so
            // anything embedding it aligns to 16 too and its size rounds up:
            //
            //   graphene_plane_t   { vec3; float }           16 + 4 -> 32
            //   graphene_euler_t   { vec3; enum }            16 + 4 -> 32
            //   graphene_sphere_t  { vec3; float }           16 + 4 -> 32
            //   graphene_ray_t     { vec3; vec3 }            16 + 16 = 32
            //   graphene_frustum_t { plane[6] }              6 * 32  = 192
            //   graphene_box_t     { vec3; vec3 }            16 + 16 = 32
            //   graphene_matrix_t  { simd4x4f }              4 * 16  = 64
            //
            // These are what every caller-allocates out parameter of those types
            // allocates before handing the pointer to graphene, so an
            // underestimate is a heap overrun on each of a few dozen calls.
            Run(() =>
            {
                Assert.Equal(16u, Graphene.Vec3.abi_info.Align);
                Assert.Equal(16u, Graphene.Vec3.abi_info.Size);

                Assert.Equal(32u, Graphene.Plane.abi_info.Size);
                Assert.Equal(32u, Graphene.Euler.abi_info.Size);
                Assert.Equal(32u, Graphene.Sphere.abi_info.Size);
                Assert.Equal(32u, Graphene.Ray.abi_info.Size);
                Assert.Equal(32u, Graphene.Box.abi_info.Size);
                Assert.Equal(192u, Graphene.Frustum.abi_info.Size);
                Assert.Equal(64u, Graphene.Matrix.abi_info.Size);

                // A field after the vector sits at 16, not at 12: the padding is
                // where graphene actually put it, which is what the frustum test
                // above measures the hard way.
                Assert.Equal(16u, Graphene.Plane.abi_info.GetFieldOffset("constant"));
                Assert.Equal(16u, Graphene.Ray.abi_info.GetFieldOffset("direction"));
            });
        }

        [Fact]
        public void Caller_allocated_results_are_freed_by_the_allocator_that_made_them()
        {
            // The regression test for a heap corruption, and it has to be
            // written this way because no single call reproduces it.
            //
            // A caller-allocates out parameter is a block this side provides and
            // the wrapper then owns, so it ends up at the type's own free
            // function. Graphene allocates everything holding a SIMD vector with
            // graphene_aligned_alloc -- _aligned_malloc where the compiler has
            // it -- and frees it with _aligned_free, which cannot be given a
            // g_malloc pointer. Every Multiply, Inverse, Cross, Normalize and
            // Negate handed it one.
            //
            // Nothing happens at the call. The generated Opaque finalizer queues
            // its free onto a 50 ms main-loop timeout, so the corruption lands
            // whenever the GC ran and the loop turned -- which in a test run is
            // some unrelated test, minutes later, as a bare "test host process
            // crashed" with exit code 0xC0000374 and an empty log. It took a
            // forced collection plus a main loop to make it certain.
            //
            // graphene_rect_t is the one that was always fine: it needs no
            // alignment, so its allocator is plain calloc and its free is plain
            // free. That is why 871 tests could pass over it.
            Run(() =>
            {
                for (int i = 0; i < 60; i++)
                {
                    var m = Scaling(2, 3, 4).Multiply(Translation(i, 1, 1));
                    Graphene.Matrix inverse;
                    m.Inverse(out inverse);
                    m.Transpose();

                    V3(1, 2, 3).Cross(V3(4, 5, i + 1));
                    V3(3, 4, i).Normalize();

                    var plane = new Graphene.Plane();
                    plane.Init(V3(0, 1, 0), -i);
                    plane.Negate();

                    RectAt(0, 0, 10, 10).Union(RectAt(i, i, 10, 10));
                }

                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();

                // The queued frees are on a 50 ms timeout, so the loop has to
                // turn for longer than that before any of them has run.
                var clock = System.Diagnostics.Stopwatch.StartNew();
                while (clock.ElapsedMilliseconds < 250)
                {
                    while (Gtk.Application.EventsPending())
                        Gtk.Application.RunIteration(false);
                    System.Threading.Thread.Sleep(1);
                }

                // Surviving is not the assertion -- a corrupted heap does not
                // always fault at once. The assertion is that graphene still
                // gives the right answers afterwards, out of an allocator whose
                // bookkeeping every one of those frees passed through.
                var after = Scaling(2, 2, 2).Multiply(Translation(1, 2, 3));
                Assert.Equal(new float[] { 1, 2, 3 },
                             new[] { after.XTranslation, after.YTranslation, after.ZTranslation });
                Assert.Equal(5f, V3(3, 4, 0).Length(), 4);
            });
        }

        // ================================================= Euler and Quaternion

        [Fact]
        public void Slerp_is_exact_at_both_ends_and_halves_the_angle_in_between()
        {
            // Unlike graphene_matrix_interpolate, spherical interpolation of
            // quaternions has no decompose-and-recompose in the way, so t = 0 is
            // exact to the last bit. t = 1 is NOT, quite: graphene evaluates
            // sin((1-t)w)/sin(w) and sin(tw)/sin(w) rather than special-casing
            // the ends, and the second of those is 1 only to within rounding --
            // about 5e-8 here, one part in 1.4e7, which is a couple of ulp of a
            // float. Close enough for an animation and not close enough for an
            // equality test, which is the point.
            //
            // Halfway between no rotation and a quarter turn is an eighth turn,
            // and the quaternion for an angle a about z is (0,0,sin(a/2),cos(a/2)).
            Run(() =>
            {
                var none = new Graphene.Quaternion();
                none.InitFromAngleVec3(0, V3(0, 0, 1));
                var quarter = new Graphene.Quaternion();
                quarter.InitFromAngleVec3(90, V3(0, 0, 1));

                Assert.Equal(none.ToVec4().ToFloat(), none.Slerp(quarter, 0).ToVec4().ToFloat());

                var atOne = none.Slerp(quarter, 1).ToVec4().ToFloat();
                Assert.NotEqual(quarter.ToVec4().ToFloat(), atOne);
                for (int i = 0; i < 4; i++)
                    Assert.Equal(quarter.ToVec4().ToFloat()[i], atOne[i], 6);

                var half = none.Slerp(quarter, 0.5f);
                float angle;
                Graphene.Vec3 axis;
                half.ToAngleVec3(out angle, out axis);
                Assert.Equal(45f, angle, 3);
                Assert.Equal(new float[] { 0, 0, 1 }, axis.ToFloat().Select(f => (float)Math.Round(f, 4)).ToArray());

                Assert.Equal((float)Math.Sin(22.5 * Math.PI / 180), half.ToVec4().Z, 4);
                Assert.Equal((float)Math.Cos(22.5 * Math.PI / 180), half.ToVec4().W, 4);
            });
        }

        [Fact]
        public void A_quaternion_times_its_inverse_is_the_identity()
        {
            Run(() =>
            {
                var q = new Graphene.Quaternion();
                q.InitFromAngleVec3(73, V3(1, 2, 3).Normalize());

                // A rotation quaternion is a unit quaternion, and the dot product
                // of a unit quaternion with itself is 1.
                Assert.Equal(1f, q.Dot(q), 4);

                var identity = new Graphene.Quaternion();
                identity.InitIdentity();
                Assert.Equal(new float[] { 0, 0, 0, 1 }, identity.ToVec4().ToFloat());

                foreach (var product in new[] { q.Invert().Multiply(q), q.Multiply(q.Invert()) })
                    Assert.Equal(new float[] { 0, 0, 0, 1 },
                                 product.ToVec4().ToFloat().Select(f => (float)Math.Round(f, 4)).ToArray());
            });
        }

        [Fact]
        public void A_quaternion_and_the_rotation_matrix_it_makes_agree_both_ways()
        {
            // graphene_quaternion_equal is an exact float comparison, and a round
            // trip through a matrix may or may not survive one: under gvsbuild
            // the returned quaternion differs in the fourth decimal place, on
            // CI's vector unit it comes back identical.
            //
            // So exactness is not asserted in either direction. An earlier
            // version required the round trip to be inexact and CI failed on it,
            // which is the right outcome for a test asserting a property of the
            // hardware rather than of the library.
            //
            // The dot product is the measure that means something: it is the
            // cosine of half the angle between two rotations, so 1 says they are
            // the same rotation whether or not the bits agree.
            Run(() =>
            {
                var q = new Graphene.Quaternion();
                q.InitFromAngleVec3(90, V3(0, 0, 1));

                Assert.True(q.ToMatrix().Near(RotationZ(90), 1e-4f),
                            "a quaternion's matrix disagreed with graphene_matrix_init_rotate");

                var back = new Graphene.Quaternion();
                back.InitFromMatrix(RotationZ(90));

                Assert.Equal(1f, back.Dot(q), 5);

                // Which is to say the two rotate a point to the same place.
                var byQuaternion = q.ToMatrix().TransformPoint(Pt(10, 0));
                var byMatrix = back.ToMatrix().TransformPoint(Pt(10, 0));
                Assert.Equal(byQuaternion.X, byMatrix.X, 3);
                Assert.Equal(byQuaternion.Y, byMatrix.Y, 3);
            });
        }

        [Fact]
        public void Euler_angles_survive_a_trip_through_a_quaternion_and_a_matrix()
        {
            Run(() =>
            {
                var euler = new Graphene.Euler();
                euler.InitWithOrder(10, 20, 30, Graphene.EulerOrder.Sxyz);

                var viaQuaternion = new Graphene.Euler();
                viaQuaternion.InitFromQuaternion(euler.ToQuaternion(), Graphene.EulerOrder.Sxyz);
                Assert.Equal(10f, viaQuaternion.X, 3);
                Assert.Equal(20f, viaQuaternion.Y, 3);
                Assert.Equal(30f, viaQuaternion.Z, 3);

                var viaMatrix = new Graphene.Euler();
                viaMatrix.InitFromMatrix(euler.ToMatrix(), Graphene.EulerOrder.Sxyz);
                Assert.Equal(10f, viaMatrix.X, 3);
                Assert.Equal(20f, viaMatrix.Y, 3);
                Assert.Equal(30f, viaMatrix.Z, 3);

                // Reordering into the same order is a no-op, and that is the
                // only case where it behaves.
                var same = euler.Reorder(Graphene.EulerOrder.Sxyz);
                Assert.True(same.ToMatrix().Near(euler.ToMatrix(), 1e-4f));
            });
        }

        [Fact]
        public void Reordering_an_euler_angle_does_not_preserve_the_rotation()
        {
            // The obvious expectation, and the wrong one. "Reorder" reads like a
            // change of spelling -- the same rotation described against a
            // different sequence of axes -- and graphene_euler_reorder is
            // documented as exactly that. It is not what happens.
            //
            // The textbook identity is that an intrinsic z-y-x rotation by
            // (c,b,a) is the same rotation as an extrinsic x-y-z one by (a,b,c).
            // Graphene agrees on the ANGLES -- reordering Sxyz(10,20,30) into
            // Rzyx gives exactly (30,20,10) -- and then its own to_matrix
            // produces a different matrix from the two, so the reordered angle
            // describes a different rotation. Of the thirty-one orders, only the
            // ones that are Sxyz under another name survive the round trip.
            //
            // So a caller normalising a set of euler angles into a preferred
            // order silently rotates the object.
            Run(() =>
            {
                var euler = new Graphene.Euler();
                euler.InitWithOrder(10, 20, 30, Graphene.EulerOrder.Sxyz);

                var reordered = euler.Reorder(Graphene.EulerOrder.Rzyx);
                Assert.Equal(Graphene.EulerOrder.Rzyx, reordered.Order);
                Assert.Equal(30f, reordered.X, 4);
                Assert.Equal(20f, reordered.Y, 4);
                Assert.Equal(10f, reordered.Z, 4);

                Assert.False(reordered.ToMatrix().Near(euler.ToMatrix(), 1e-2f),
                             "graphene_euler_reorder now preserves the rotation; this test records that it did not");

                // The two matrices are not unrelated -- they are each other's
                // 3x3 anti-transpose, which is what applying the sequence
                // backwards produces.
                var forward = euler.ToMatrix();
                var backward = reordered.ToMatrix();
                for (uint row = 0; row < 3; row++)
                    for (uint col = 0; col < 3; col++)
                        Assert.Equal(forward.GetValue(row, col), backward.GetValue(2 - col, 2 - row), 4);
            });
        }

        [Fact]
        public void Alpha_beta_and_gamma_follow_the_rotation_order_rather_than_x_y_z()
        {
            // X, Y and Z are the angles about the fixed axes; alpha, beta and
            // gamma are the FIRST, SECOND and THIRD rotations applied, in the
            // order the euler angle carries. For Sxyz they coincide, which is
            // exactly why reading alpha as "the x angle" survives testing -- and
            // for Ryxz alpha is the y angle, so a caller who assumes otherwise
            // gets two axes swapped and no error.
            //
            // The values are in radians while X, Y and Z are in degrees, which is
            // the second half of the same trap.
            Run(() =>
            {
                var xyz = new Graphene.Euler();
                xyz.InitWithOrder(10, 20, 30, Graphene.EulerOrder.Sxyz);
                Assert.Equal(10 * Math.PI / 180, xyz.Alpha, 5);
                Assert.Equal(20 * Math.PI / 180, xyz.Beta, 5);
                Assert.Equal(30 * Math.PI / 180, xyz.Gamma, 5);

                var yxz = new Graphene.Euler();
                yxz.InitWithOrder(10, 20, 30, Graphene.EulerOrder.Ryxz);
                Assert.Equal(10f, yxz.X, 4);
                Assert.Equal(20f, yxz.Y, 4);

                Assert.Equal(20 * Math.PI / 180, yxz.Alpha, 5);   // y comes first
                Assert.Equal(10 * Math.PI / 180, yxz.Beta, 5);    // then x
                Assert.Equal(30 * Math.PI / 180, yxz.Gamma, 5);
            });
        }

        // ================================================================== Gsk

        [Fact]
        public void A_transform_chain_applies_its_last_operation_to_the_point_first()
        {
            // GskTransform composes the way a CSS transform list does, which is
            // the opposite of the order the calls are written in: the point meets
            // the LAST operation in the chain first. Here (1,1) is scaled to
            // (2,3) and then translated to (12,23) -- not translated to (11,21)
            // and then scaled to (22,63).
            Run(() =>
            {
                var transform = new Gsk.Transform().Translate(Pt(10, 20)).Scale(2, 3);

                var moved = transform.Point(Pt(1, 1));
                Assert.Equal(12f, moved.X, 4);
                Assert.Equal(23f, moved.Y, 4);

                var bounds = transform.Bounds(RectAt(0, 0, 1, 1));
                Assert.Equal(10f, bounds.X, 4);
                Assert.Equal(20f, bounds.Y, 4);
                Assert.Equal(2f, bounds.Width, 4);
                Assert.Equal(3f, bounds.Height, 4);

                // The equivalent graphene matrix says the same thing, in
                // graphene's row-vector layout: scale on the diagonal,
                // translation in the last row.
                Assert.Equal(new float[]
                {
                    2, 0, 0, 0,
                    0, 3, 0, 0,
                    0, 0, 1, 0,
                    10, 20, 0, 1,
                }, transform.ToMatrix().ToFloat());
            });
        }

        [Fact]
        public void A_transform_and_its_inverse_return_a_point_untouched()
        {
            Run(() =>
            {
                var transform = new Gsk.Transform().Translate(Pt(10, 20)).Rotate(30).Scale(2, 3);
                var inverse = transform.Invert();

                var point = Pt(7, -4);
                var there = transform.Point(point);
                var back = inverse.Point(there);

                Assert.Equal(point.X, back.X, 3);
                Assert.Equal(point.Y, back.Y, 3);

                // The transform actually moved it, so the round trip is not
                // trivially satisfied by an inverse that does nothing.
                Assert.True(Math.Abs(there.X - point.X) > 1,
                            "the transform left the point where it was; the round trip proves nothing");
            });
        }

        [Fact]
        public void A_transform_reports_its_components_three_different_ways()
        {
            Run(() =>
            {
                var affine = new Gsk.Transform().Translate(Pt(10, 20)).Scale(2, 3);

                float scaleX, scaleY, dx, dy;
                affine.ToAffine(out scaleX, out scaleY, out dx, out dy);
                Assert.Equal(2f, scaleX, 4);
                Assert.Equal(3f, scaleY, 4);
                Assert.Equal(10f, dx, 4);
                Assert.Equal(20f, dy, 4);

                float xx, yx, xy, yy, d2x, d2y;
                affine.To2d(out xx, out yx, out xy, out yy, out d2x, out d2y);
                Assert.Equal(new float[] { 2, 0, 0, 3, 10, 20 }, new[] { xx, yx, xy, yy, d2x, d2y });

                float tx, ty;
                new Gsk.Transform().Translate(Pt(4, 5)).ToTranslate(out tx, out ty);
                Assert.Equal(4f, tx, 4);
                Assert.Equal(5f, ty, 4);

                // The decomposed form separates a rotation out, which the affine
                // form cannot express.
                float skewX, skewY, cScaleX, cScaleY, angle, cdx, cdy;
                new Gsk.Transform().Translate(Pt(1, 2)).Rotate(90).Scale(2, 2)
                    .To2dComponents(out skewX, out skewY, out cScaleX, out cScaleY, out angle, out cdx, out cdy);
                Assert.Equal(90f, angle, 3);
                Assert.Equal(2f, cScaleX, 4);
                Assert.Equal(1f, cdx, 4);
                Assert.Equal(2f, cdy, 4);
                Assert.Equal(0f, skewX, 4);
            });
        }

        [Fact]
        public void A_transform_built_from_a_matrix_forgets_what_the_matrix_was()
        {
            // GskTransform's category is a record of how it was BUILT, not an
            // analysis of what it does. Handing it a matrix that is a plain
            // scale gives category Unknown, so a renderer's fast path for an
            // affine transform is missed -- while the matrix itself comes back
            // element for element.
            Run(() =>
            {
                var scale = Scaling(2, 3, 4);
                var fromMatrix = new Gsk.Transform().Matrix(scale);

                Assert.Equal(Gsk.TransformCategory.Unknown, fromMatrix.Category);
                Assert.Equal(scale.ToFloat(), fromMatrix.ToMatrix().ToFloat());

                // The same transform expressed through the builder is classified.
                Assert.Equal(Gsk.TransformCategory.TwoDAffine,
                             new Gsk.Transform().Scale(2, 3).Category);
            });
        }

        [Fact]
        public void A_skew_shifts_one_axis_in_proportion_to_the_other()
        {
            Run(() =>
            {
                // 45 degrees, so tan is 1 and every unit of y adds a unit of x.
                var skew = new Gsk.Transform().Skew(45, 0);

                Assert.Equal(1f, skew.Point(Pt(0, 1)).X, 4);
                Assert.Equal(1f, skew.Point(Pt(0, 1)).Y, 4);
                Assert.Equal(3f, skew.Point(Pt(1, 2)).X, 4);

                // A point on the x axis has no y to shear, so it does not move.
                Assert.Equal(5f, skew.Point(Pt(5, 0)).X, 4);
                Assert.Equal(0f, skew.Point(Pt(5, 0)).Y, 4);

                // Perspective puts -1/depth in the element that divides by z,
                // which is the only place in the matrix it appears.
                var perspective = new Gsk.Transform().Perspective(100).ToMatrix();
                Assert.Equal(-0.01f, perspective.GetValue(2, 3), 5);
                Assert.Equal(1f, perspective.GetValue(0, 0), 5);
            });
        }

        [Fact]
        public void Node_bounds_grow_by_exactly_what_the_operation_needs()
        {
            // Every render node computes its bounds from its child's, so these
            // are arithmetic over a rectangle chosen here. A blur has to reach
            // 1.5 radii beyond the edge to gather the pixels it averages, while
            // opacity and a debug label change nothing at all -- and a node that
            // widened its bounds when it did not need to would make GTK repaint
            // more than it must, silently.
            Run(() =>
            {
                var red = new Gdk.RGBA { Red = 1f, Alpha = 1f };
                var child = new Gsk.ColorNode(red, RectAt(0, 0, 10, 10));

                Assert.Equal(Gsk.RenderNodeType.ColorNode, child.NodeType);
                Assert.Equal(10f, child.Bounds.Width, 4);

                var faded = new Gsk.OpacityNode(child, 0.5f);
                Assert.Equal(0f, faded.Bounds.X, 4);
                Assert.Equal(10f, faded.Bounds.Width, 4);

                var labelled = new Gsk.DebugNode(child, "why this node is here");
                Assert.Equal("why this node is here", labelled.Message);
                Assert.Equal(10f, labelled.Bounds.Width, 4);

                var blurred = new Gsk.BlurNode(child, 4);
                Assert.Equal(-6f, blurred.Bounds.X, 4);          // 1.5 * 4
                Assert.Equal(22f, blurred.Bounds.Width, 4);      // 10 + 2 * 6

                var doubled = new Gsk.TransformNode(child, new Gsk.Transform().Scale(2, 2));
                Assert.Equal(20f, doubled.Bounds.Width, 4);
                Assert.Equal(20f, doubled.Bounds.Height, 4);

                var clipped = new Gsk.ClipNode(blurred, RectAt(0, 0, 5, 5));
                Assert.Equal(0f, clipped.Bounds.X, 4);
                Assert.Equal(5f, clipped.Bounds.Width, 4);
            });
        }
    }
}
