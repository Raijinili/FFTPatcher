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

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using PatcherLib.Utilities;

namespace FFTPatcher.TextEditor
{
    /// <summary>
    /// An editor that edits lists of strings.
    /// </summary>
    partial class StringListEditor : UserControl
    {
        private IFile boundFile;
        private int boundSection;

        private bool ignoreChanges = false;
#if MEASURESTRINGS
        public int TextColumnIndex { get { return textColumn.Index; } }
#else
        public const int TextColumnIndex = 2;
#endif

        /// <summary>
        /// Gets the current row.
        /// </summary>
        /// <value>The current row.</value>
        public int CurrentRow
        {
            get { return dataGridView.CurrentRow != null ? (int)dataGridView.CurrentRow.Cells[numberColumn.Name].Value : -1; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StringListEditor"/> class.
        /// </summary>
        public StringListEditor()
        {
            InitializeComponent();
            dataGridView.CellEndEdit += dataGridView_CellEndEdit;
            dataGridView.EditingControlShowing += dataGridView_EditingControlShowing;
            dataGridView.CellValidating += new DataGridViewCellValidatingEventHandler( dataGridView_CellValidating );
            dataGridView.CellValidated += new DataGridViewCellEventHandler( dataGridView_CellValidated );
            textColumn.DefaultCellStyle.Font = new Font( "Arial Unicode MS", 9 );
#if !MEASURESTRINGS
            dataGridView.Columns.Remove(widthColumn);
#else
            widthColumn.DefaultCellStyle.Font = new Font( "Arial Unicode MS", 9 );
#endif
        }

        private void dataGridView_CellValidating( object sender, DataGridViewCellValidatingEventArgs e )
        {
            if ( !ignoreChanges &&
                e.ColumnIndex == TextColumnIndex &&
                CellValidating != null )
            {
                CellValidating( this, e );
            }
        }

        private void dataGridView_CellValidated( object sender, DataGridViewCellEventArgs e )
        {
            if ( !ignoreChanges && 
                 e.ColumnIndex == TextColumnIndex )
            {
                string s = (string)dataGridView[e.ColumnIndex, e.RowIndex].Value ?? string.Empty;
                boundFile[boundSection, CurrentRow] = s;
#if MEASURESTRINGS
                dataGridView[widthColumn.Index, e.RowIndex].Value = GetWidthColumnString( s );
#endif
            }
        }

#if MEASURESTRINGS
        private string GetWidthColumnString( string s )
        {
            var widths = boundFile.CharMap.MeasureEachLineInFont( s, font );
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            var widthStrings = new List<string>( widths.Count );

            widths.ForEach( w => widthStrings.Add( string.Format( "{0}", w ) ) );
            return string.Join( Environment.NewLine, widthStrings.ToArray() );
        }
#endif

        public event DataGridViewCellValidatingEventHandler CellValidating;

        /// <summary>
        /// Occurs when text in a textbox has changed.
        /// </summary>
        public event EventHandler TextBoxTextChanged;

        private void dataGridView_CellEndEdit( object sender, DataGridViewCellEventArgs e )
        {
            if( dataGridView.EditingControl is TextBox )
            {
                dataGridView.EditingControl.TextChanged -= tb_TextChanged;
            }
        }

        private void dataGridView_EditingControlShowing( object sender, DataGridViewEditingControlShowingEventArgs e )
        {
            if( !ignoreChanges &&
                dataGridView.CurrentCell != null && 
                dataGridView.CurrentCell.ColumnIndex == textColumn.Index && 
                dataGridView.EditingControl is TextBox )
            {
                TextBox tb = dataGridView.EditingControl as TextBox;
                tb.Font = new Font( "Arial Unicode MS", 9 );
                tb.TextChanged += tb_TextChanged;
            }
        }

        private void tb_TextChanged( object sender, EventArgs e )
        {
            if( !ignoreChanges && 
                TextBoxTextChanged != null )
            {
                TextBoxTextChanged( sender, e );
            }
        }

#if MEASURESTRINGS
        PatcherLib.Datatypes.FFTFont font;
#endif
        
        /// <summary>
        /// Binds this editor to a list of strings.
        /// </summary>
        public void BindTo( IList<string> names, IFile file, int section )
        {
            ignoreChanges = true;
            SuspendLayout();
            int count = file.SectionLengths[section];
            List<string> ourNames = new List<string>( names );
            for ( int i = names.Count; i < count; i++ )
            {
                ourNames.Add( string.Empty );
            }

            IList<int> disallowed = ( file is ISerializableFile ) ? ( (ISerializableFile)file ).Layout.DisallowedEntries[section] : null;

            DataGridViewRow[] rows = new DataGridViewRow[count];
            dataGridView.SuspendLayout();
#if MEASURESTRINGS
            font = file.Context == PatcherLib.Datatypes.Context.US_PSP ? TextUtilities.PSPFont : TextUtilities.PSXFont;
#endif
            boundFile = file;

            for( int i = 0; i < count; i++ )
            {
                DataGridViewRow row = new DataGridViewRow();
#if MEASURESTRINGS
                row.CreateCells( dataGridView, i, ourNames[i],
                    GetWidthColumnString( file[section, i] ?? string.Empty ), file[section, i] );

#else
                row.CreateCells(dataGridView, i, ourNames[i], file[section, i]);
#endif
                row.ReadOnly = disallowed != null && disallowed.Count > 0 && disallowed.Contains( i );
                rows[i] = row;
            }
            dataGridView.Rows.Clear();
            dataGridView.Rows.AddRange( rows );
            dataGridView.ResumeLayout();

            bool showSeparatorChoices = file is ISerializableFile && (file as ISerializableFile).Layout.AllowedTerminators.Count > 1;
            separatorComboBox.Visible = showSeparatorChoices;
            separatorComboBox.Enabled = showSeparatorChoices;
            separatorLabel.Visible = showSeparatorChoices;
            if (showSeparatorChoices)
            {
                PatcherLib.Datatypes.Set<byte> seps = (file as ISerializableFile).Layout.AllowedTerminators;
                separatorComboBox.BeginUpdate();
                separatorComboBox.Items.Clear();
                separatorComboBox.FormatString = "X2";
                seps.ForEach(b => separatorComboBox.Items.Add(b));
                separatorComboBox.SelectedItem = file.SelectedTerminator;
                separatorComboBox.EndUpdate();
            }

            boundSection = section;
            ignoreChanges = false;
            ResumeLayout();
        }

        private void separatorComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (separatorComboBox.Enabled && separatorComboBox.Visible && !ignoreChanges)
            {
                boundFile.SelectedTerminator = (byte)separatorComboBox.SelectedItem;
            }
        }

    }
}
