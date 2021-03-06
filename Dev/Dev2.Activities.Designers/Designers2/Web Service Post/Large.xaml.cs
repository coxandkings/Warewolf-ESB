﻿
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Windows;

namespace Dev2.Activities.Designers2.Web_Service_Post
{
    // Interaction logic for Large.xaml
    public partial class Large
    {
        public Large()
        {
            InitializeComponent();
        }

        #region Overrides of ActivityDesignerTemplate

        protected override IInputElement GetInitialFocusElement()
        {
            return MainGrid;
        }

        #endregion

        void RequestBody_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            var viewModel = DataContext as WebServicePostViewModel;

            if (viewModel != null)
            {
                var dataContext = viewModel.InputArea;
                if (dataContext.IsEnabled)
                {
                    //MinHeight = dataContext.MinHeight + 25;
                    //MaxHeight += e.NewSize.Height;
                    //Height = MinHeight;
                }
            }
        }
    }
}
