using System;
using Foundation;
using UIKit;

namespace iOSApplication_0._5._3
{
    /// <summary>
    /// Responsible for populating the 'Select movie' table view.
    /// </summary>
    public class TableSource : UITableViewSource
    {
        internal string[] TableItems;
        internal string[] SubTableItems;
        internal string CellIdentifier = "TableCell";
        readonly ViewController _owner;
        internal static string SelectedMovie;

        /// <summary>
        /// The 'Select movie' table source.
        /// </summary>
        /// <param name="items"></param>
        /// <param name="owner"></param>
        public TableSource(string[] items, string [] subitems, ViewController owner)
        {
            TableItems = items;
            SubTableItems = subitems;
            _owner = owner;

        }

        /// <summary>
        /// Called by the TableView to determine how many sections(groups) there are.
        /// </summary>
        public override nint NumberOfSections(UITableView tableView)
        {
            return 1;
        }

        /// <summary>
        /// Called by the TableView to determine how many cells to create for that particular section.
        /// </summary>
        public override nint RowsInSection(UITableView tableview, nint section)
        {
            return TableItems.Length;
        }

        /// <summary>
        /// Called when a row is touched
        /// </summary>
        public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
        {
            UIAlertController okAlertController = UIAlertController.Create("Row Selected", TableItems[indexPath.Row], UIAlertControllerStyle.Alert);
            okAlertController.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, null));
            //owner.PresentViewController(okAlertController, true, null);
            SelectedMovie = TableItems[indexPath.Row];
            _owner.SetSelectedMovie(SelectedMovie);
            tableView.DeselectRow(indexPath, true);
            tableView.Hidden = true;
            _owner.SetForegroundLabel("Selected movie: " + SelectedMovie);
        }

        /// <summary>
        /// Called by the TableView to get the actual UITableViewCell to render for the particular row
        /// </summary>
        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            UITableViewCell cell = tableView.DequeueReusableCell(CellIdentifier);
            string item = TableItems[indexPath.Row];
            string subitem = SubTableItems[indexPath.Row];
            //---- if there are no cells to reuse, create a new one
            if (cell == null)
            { cell = new UITableViewCell(UITableViewCellStyle.Subtitle, CellIdentifier); }

            cell.TextLabel.Text = item;
            cell.DetailTextLabel.Text = subitem;

            return cell;
        }

        //		public override string TitleForHeader (UITableView tableView, nint section)
        //		{
        //			return " ";
        //		}

    }
}