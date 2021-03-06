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
using FFTPatcher.Controls;
using FFTPatcher.Datatypes;
using PatcherLib.Datatypes;

namespace FFTPatcher.Editors
{
    public partial class SkillSetEditor : BaseEditor
    {
		#region Instance Variables (5) 

        private List<ComboBoxWithDefault> actionComboBoxes;
        private bool ignoreChanges = false;
        private Context ourContext = Context.Default;
        private SkillSet skillSet;
        private List<ComboBoxWithDefault> theRestComboBoxes;

		#endregion Instance Variables 

		#region Public Properties (1) 

        public SkillSet SkillSet
        {
            get { return skillSet; }
            set
            {
                if( value == null )
                {
                    this.Enabled = false;
                    skillSet = null;
                }
                else if( skillSet != value )
                {
                    this.Enabled = true;
                    skillSet = value;
                    UpdateView();
                }
            }
        }

		#endregion Public Properties 

		#region Constructors (1) 

        public SkillSetEditor()
        {
            InitializeComponent();
            actionComboBoxes = new List<ComboBoxWithDefault>( new ComboBoxWithDefault[] { 
                actionComboBox1, actionComboBox2, actionComboBox3, actionComboBox4, 
                actionComboBox5, actionComboBox6, actionComboBox7, actionComboBox8, 
                actionComboBox9, actionComboBox10, actionComboBox11, actionComboBox12, 
                actionComboBox13, actionComboBox14, actionComboBox15, actionComboBox16 } );
            theRestComboBoxes = new List<ComboBoxWithDefault>( new ComboBoxWithDefault[] {
                theRestComboBox1, theRestComboBox2, theRestComboBox3,
                theRestComboBox4, theRestComboBox5, theRestComboBox6 } );
        }

		#endregion Constructors 

		#region Public Methods (1) 

        public void UpdateView()
        {        	
            this.ignoreChanges = true;
            this.SuspendLayout();
            actionGroupBox.SuspendLayout();
            theRestGroupBox.SuspendLayout();

            if( ourContext != FFTPatch.Context )
            {
                ourContext = FFTPatch.Context;
                
                foreach( ComboBoxWithDefault actionComboBox in actionComboBoxes )
                {
                    actionComboBox.Items.Clear();
                    actionComboBox.Items.AddRange( AllAbilities.DummyAbilities );
                    actionComboBox.SelectedIndexChanged += actionComboBox_SelectedIndexChanged;
                }
                foreach( ComboBoxWithDefault theRestComboBox in theRestComboBoxes )
                {
                    theRestComboBox.Items.Clear();
                    theRestComboBox.Items.AddRange( AllAbilities.DummyAbilities );
                    theRestComboBox.SelectedIndexChanged += theRestComboBox_SelectedIndexChanged;
                }
            }
            for( int i = 0; i < 16; i++ )
            {
                actionComboBoxes[i].SetValueAndDefault( skillSet.Actions[i], skillSet.Default.Actions[i] );
            }
            for( int i = 0; i < 6; i++ )
            {
                theRestComboBoxes[i].SetValueAndDefault( skillSet.TheRest[i], skillSet.Default.TheRest[i] );
            }

            theRestGroupBox.ResumeLayout();
            actionGroupBox.ResumeLayout();
            this.ResumeLayout();
            this.ignoreChanges = false;
        }

		#endregion Public Methods 

		#region Private Methods (2) 

        private void actionComboBox_SelectedIndexChanged( object sender, EventArgs e )
        {
            if( !ignoreChanges )
            {
                ComboBoxWithDefault c = sender as ComboBoxWithDefault;
                int i = actionComboBoxes.IndexOf( c );
                skillSet.Actions[i] = c.SelectedItem as Ability;
                OnDataChanged( this, System.EventArgs.Empty );
            }
        }

        private void theRestComboBox_SelectedIndexChanged( object sender, EventArgs e )
        {
            if( !ignoreChanges )
            {
                ComboBoxWithDefault c = sender as ComboBoxWithDefault;
                int i = theRestComboBoxes.IndexOf( c );
                skillSet.TheRest[i] = c.SelectedItem as Ability;
                OnDataChanged( this, System.EventArgs.Empty );
            }
        }

		#endregion Private Methods 
    }
}
