// Gtk.RadioButton - the Gtk 3 radio button, over Gtk 4's grouped GtkCheckButton
//
// This program is free software; you can redistribute it and/or
// modify it under the terms of version 2 of the Lesser GNU General
// Public License as published by the Free Software Foundation.

namespace Gtk {

	using System;
	using System.Collections.Generic;

	/// <summary>
	/// Stands in for GtkRadioButton, which Gtk 4 removed.
	/// </summary>
	/// <remarks>
	/// <para>The replacement is not a different widget so much as a different spelling: Gtk 4
	/// folded radio behaviour into GtkCheckButton, where calling
	/// <c>gtk_check_button_set_group</c> turns a check button into a member of a
	/// mutually-exclusive group and gives it the radio appearance. So this really is the same
	/// control, and the mapping is exact.</para>
	/// <para>What is NOT exact is <see cref="Group"/>. Gtk 3 handed out a GSList of every member,
	/// which callers walked to find the active one. Gtk 4 exposes only "the group I joined", and
	/// there is no way to ask it for its members - so the membership is tracked here, in managed
	/// code, and is accurate for groups built through this class. A group assembled by calling
	/// <c>SetGroup</c> on the underlying check button directly will not appear.</para>
	/// </remarks>
	public class RadioButton : CheckButton {

		// The group's members, keyed by the button that founded it. A List rather than a set
		// because Gtk 3's GSList had an order and callers indexed into it.
		static readonly Dictionary<RadioButton, List<RadioButton>> _groups =
			new Dictionary<RadioButton, List<RadioButton>>();

		RadioButton _groupOwner;

		public RadioButton() : base()
		{
			JoinGroup(null);
		}

		public RadioButton(string label) : base()
		{
			Label = label;
			JoinGroup(null);
		}

		public RadioButton(RadioButton radioGroupMember) : base()
		{
			JoinGroup(radioGroupMember);
		}

		public RadioButton(RadioButton radioGroupMember, string label) : base()
		{
			Label = label;
			JoinGroup(radioGroupMember);
		}

		protected RadioButton(IntPtr raw) : base(raw)
		{
		}

		void JoinGroup(RadioButton member)
		{
			_groupOwner = member == null ? this : member._groupOwner;

			List<RadioButton> members;

			if (!_groups.TryGetValue(_groupOwner, out members)) {
				members = new List<RadioButton>();
				_groups[_groupOwner] = members;
			}

			members.Add(this);

			// Gtk 4 wants the group leader, not the previous member. Passing the leader keeps a
			// chain of buttons each constructed from the last in one group, which is how Gtk 3
			// code builds them.
			// base.Group, not this.Group: CheckButton exposes gtk_check_button_set_group as a
			// set-only property of that name, which the members list below shadows.
			if (member != null)
				base.Group = _groupOwner;
		}

		/// <summary>
		/// The buttons sharing this one's group, including itself.
		/// </summary>
		/// <remarks>
		/// An array rather than Gtk 3's GSList: there is no native list behind it to wrap, and
		/// handing back a copy means removing a button while iterating is safe.
		/// </remarks>
		public new RadioButton[] Group {
			get {
				List<RadioButton> members;

				if (_groupOwner == null || !_groups.TryGetValue(_groupOwner, out members))
					return new[] { this };

				return members.ToArray();
			}
		}

		/// <summary>
		/// Stands in for GtkButton::clicked on a radio button, over GtkCheckButton::toggled.
		/// </summary>
		/// <remarks>
		/// Gtk 3's GtkRadioButton descended from GtkButton and so had ::clicked; Gtk 4's
		/// GtkCheckButton does not, and ::toggled is what it offers. The two differ: ::clicked
		/// fired for every press including one that re-selected the already-active button, while
		/// ::toggled fires only when the active state actually changes. That is the better signal
		/// for a radio group, and the one Gtk 4 intends.
		/// </remarks>
		// System.EventHandler, qualified: Gtk 4 introduces a Gtk.EventHandler that otherwise wins
		// name resolution inside namespace Gtk. Same trap as Gtk.Application.Invoke.
		public event System.EventHandler Clicked {
			add { Toggled += value; }
			remove { Toggled -= value; }
		}

		/// <summary>Stands in for GtkButton:relief. See <see cref="ReliefStyle"/>.</summary>
		/// <remarks>
		/// Repeated here rather than inherited: Gtk 3's GtkRadioButton descended from GtkButton and
		/// so had relief and image-position, but Gtk 4's GtkCheckButton does not descend from
		/// GtkButton at all, so the copies on the compat Button are out of reach.
		/// </remarks>
		public ReliefStyle Relief {
			get { return HasCssClass("flat") ? ReliefStyle.None : ReliefStyle.Normal; }
			set {
				if (value == ReliefStyle.Normal)
					RemoveCssClass("flat");
				else
					AddCssClass("flat");
			}
		}

		/// <inheritdoc cref="Button.ImagePosition"/>
		public PositionType ImagePosition { get; set; }

		/// <inheritdoc cref="EventBox.OnDrawn"/>
		/// <remarks>
		/// Here rather than on <see cref="CheckButton"/>: this class is the Gtk 3 compatibility
		/// surface, and putting a Gtk 3 drawing vfunc on the real Gtk 4 widget would offer it to
		/// code that has no reason to want it.
		/// </remarks>
		protected virtual bool OnDrawn(Cairo.Context cr)
		{
			return false;
		}

		protected override void OnSnapshot(Snapshot snapshot)
		{
			CompatVFunc.Snapshot(this, snapshot, OnDrawn);
			base.OnSnapshot(snapshot);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && _groupOwner != null) {
				List<RadioButton> members;

				if (_groups.TryGetValue(_groupOwner, out members)) {
					members.Remove(this);

					// The dictionary is static, so a group whose last member is gone would
					// otherwise keep both its key and its list alive for the process's lifetime.
					if (members.Count == 0)
						_groups.Remove(_groupOwner);
				}

				_groupOwner = null;
			}

			base.Dispose(disposing);
		}
	}
}
