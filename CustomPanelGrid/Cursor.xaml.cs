using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace CustomPanelGrid
	{
	/// <summary>
	/// Interaction logic for Cursor.xaml
	/// </summary>
	public partial class Cursor : UserControl
		{

		public Cursor ( )
			{
			InitializeComponent ( );
			//Grid
			try
				{
				Grid MyPanel = GetParentGrid ( Frame );
				}
			catch ( System.Exception )
				{


				}

			SetGridBackground ( MyPanel );

			MyPanel.PreviewMouseLeftButtonDown+=FrameMoveOnClick;
			MyPanel.MouseLeftButtonUp+=MouseLeftButtonRelease;
			//Cursor Name=Frame
			this.PreviewKeyDown+=MoveCursor;
			this.MouseLeftButtonDown+=MouseLeftButtonHold;

			}

		#region Parent Grid Setup
		private static Grid GetParentGrid ( DependencyObject child )
			{
			if ( child==null ) return null;

			Grid foundParent = null;
			var currentParent = VisualTreeHelper.GetParent ( child );

			do
				{
				var frameworkElement = currentParent as FrameworkElement;
				if ( frameworkElement is Grid )
					{
					foundParent=(Grid) currentParent;
					break;
					}

				currentParent=VisualTreeHelper.GetParent ( currentParent );

				} while ( currentParent!=null );

			return foundParent;
			}

		private static void SetGridBackground ( Grid grid )
			{
			if ( grid.Background==null ) { grid.Background=Brushes.Transparent; }
			}
		#endregion

		internal struct GridLocation
			{
			public int Row;
			public int Column;
			public static GridLocation Substraction ( GridLocation A , GridLocation B )
				{
				return new GridLocation
					{
					Row=A.Row-B.Row ,
					Column=A.Column-B.Column
					};
				}

			}
		GridLocation activeCell;
		private void SetActiveCell ( GridLocation gridLocation )
			{
			Grid.SetColumn ( Frame , gridLocation.Column );
			Grid.SetRow ( Frame , gridLocation.Row );
			activeCell=gridLocation;
			}
		private void SetActiveCell ( )
			{
			activeCell.Column=Grid.GetColumn ( Frame );
			activeCell.Row=Grid.GetRow ( Frame );
			}
		private GridLocation GetActiveCell ( ) { return activeCell; }


		private GridLocation MoveCursor ( System.Windows.Input.KeyEventArgs e , GridLocation actualLocation )
			{
			switch ( e.Key )
				{
				case System.Windows.Input.Key.Left:
					Grid.SetColumn ( Frame , Grid.GetColumn ( Frame )-1 );
					break;
				case System.Windows.Input.Key.Up:
					Grid.SetRow ( Frame , Grid.GetRow ( Frame )-1 );
					break;
				case System.Windows.Input.Key.Right:
					Grid.SetColumn ( Frame , Grid.GetColumn ( Frame )+1 );
					break;
				case System.Windows.Input.Key.Down:
					Grid.SetRow ( Frame , Grid.GetRow ( Frame )+1 );
					break;
				}
			actualLocation.Row=Grid.GetRow ( Frame );
			actualLocation.Column=Grid.GetColumn ( Frame );
			return actualLocation;
			}
		private void MoveCursor ( object sender , System.Windows.Input.KeyEventArgs e )
			{
			GridLocation actualLocation = GetActiveCell ( );

			if ( MyPanel.RowDefinitions.Count-1>=actualLocation.Row
				&MyPanel.ColumnDefinitions.Count-1>=actualLocation.Column
				&actualLocation.Row>=0
				&actualLocation.Column>=0 )
				{
				try
					{
					actualLocation=MoveCursor ( e , actualLocation );
					}
				catch ( System.Exception ex )
					{
					SetActiveCell ( actualLocation );
					}
				finally
					{

					if ( actualLocation.Row>MyPanel.RowDefinitions.Count-1 ) { actualLocation.Row=MyPanel.RowDefinitions.Count-1; }
					if ( actualLocation.Column>MyPanel.ColumnDefinitions.Count-1 ) { actualLocation.Column=MyPanel.ColumnDefinitions.Count-1; }
					SetActiveCell ( actualLocation );
					Frame.Focus ( );

					}
				}
			e.Handled=true;
			}

		private static GridLocation GetMouseLocation ( Grid panel )
			{
			var point = Mouse.GetPosition ( panel );
			int row = 0;
			int col = 0;
			double accumulatedHeight = 0.0;
			double accumulatedWidth = 0.0;
			// calc row mouse was over
			foreach ( var rowDefinition in panel.RowDefinitions )
				{
				accumulatedHeight+=rowDefinition.ActualHeight;
				if ( accumulatedHeight>=point.Y )
					break;
				row++;
				}
			// calc col mouse was over
			foreach ( var columnDefinition in panel.ColumnDefinitions )
				{
				accumulatedWidth+=columnDefinition.ActualWidth;
				if ( accumulatedWidth>=point.X )
					break;
				col++;
				}

			GridLocation mouseLocation;
			mouseLocation.Column=col;
			mouseLocation.Row=row;
			return mouseLocation;
			}
		private void FrameMoveOnClick ( object sender , System.Windows.Input.MouseButtonEventArgs e )
			{
			MoveFrameToMouse ( );
			}
		private void MoveFrameToMouse ( )
			{
			GridLocation mouseLocation;
			mouseLocation=GetMouseLocation ( MyPanel );
			SetActiveCell ( mouseLocation );
			Frame.Focus ( );
			}

		private void MouseLeftButtonHold ( object sender , MouseButtonEventArgs e )
			{

			}
		private void MouseLeftButtonRelease ( object sender , MouseButtonEventArgs e )
			{

			}

		private void RenderTransformCursor ( GridLocation finalLocation )
			{
			GridLocation delta = GridLocation.Substraction ( GetActiveCell ( ) , finalLocation );

			GridLocation spanstart;
			GridLocation spannumber;
			if ( delta.Row>0 ) { spanstart.Row=GetActiveCell ( ).Row; } else { spanstart.Row=GetActiveCell ( ).Row; }
			if ( delta.Column>0 ) { spanstart.Column=GetActiveCell ( ).Column; } else { spanstart.Column=GetActiveCell ( ).Column; }
			spannumber.Row=Math.Abs ( delta.Row )+1;
			spannumber.Column=Math.Abs ( delta.Column )+1;

			Grid.SetColumnSpan ( Frame , spannumber.Column );
			Grid.SetRowSpan ( Frame , spannumber.Row );
			}



		}

	public class CurorErrorHandler
		{

		}
	}
