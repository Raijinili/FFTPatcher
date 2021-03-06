/*
    Copyright 2007, Joe Davidson <joedavidson@gmail.com>

    This file is part of FFTPatcher.

    FFTPatcher is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    FFTPatcher is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with FFTPatcher.  If not, see <http://www.gnu.org/licenses/>.
*/

using System.Windows.Forms;
using FFTPatcher.Datatypes;
using PatcherLib;

namespace FFTPatcher.Editors
{
    public partial class InflictStatusEditor : BaseEditor
    {
		#region Instance Variables (3) 

        private readonly string[] flags = new string[] { 
            "AllOrNothing", "Random", "Separate", "Cancel", 
            "Blank1", "Blank2", "Blank3", "Blank4" };
        private bool ignoreChanges = false;
        private InflictStatus status;

		#endregion Instance Variables 

		#region Public Properties (1) 

        public InflictStatus InflictStatus
        {
            get { return status; }
            set
            {
                if( value == null )
                {
                    status = null;
                    this.Enabled = false;
                }
                else if( value != status )
                {
                    status = value;
                    this.Enabled = true;
                    UpdateView();
                }
            }
        }

		#endregion Public Properties 

		#region Constructors (1) 

        public InflictStatusEditor()
        {
            InitializeComponent();
            flagsCheckedListBox.ItemCheck += flagsCheckedListBox_ItemCheck;
            inflictStatusesEditor.DataChanged += OnDataChanged;
        }

		#endregion Constructors 

		#region Private Methods (2) 

        private void flagsCheckedListBox_ItemCheck( object sender, ItemCheckEventArgs e )
        {
            if( !ignoreChanges )
            {
                ReflectionHelpers.SetFlag( status, flags[e.Index], e.NewValue == CheckState.Checked );
                OnDataChanged( this, System.EventArgs.Empty );
            }
        }

        public void UpdateView()
        {
            ignoreChanges = true;
            SuspendLayout();
            flagsCheckedListBox.SuspendLayout();
            inflictStatusesEditor.SuspendLayout();

            if( status.Default != null )
            {
                flagsCheckedListBox.SetValuesAndDefaults( ReflectionHelpers.GetFieldsOrProperties<bool>( status, flags ), status.Default.ToBoolArray() );
            }

            inflictStatusesEditor.Statuses = status.Statuses;
            inflictStatusesEditor.UpdateView();

            inflictStatusesEditor.ResumeLayout();
            flagsCheckedListBox.ResumeLayout();
            ResumeLayout();
            ignoreChanges = false;
        }

		#endregion Private Methods 
    }
}
