using System;
using System.Threading;
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

				//In case it doesnt find parent

				}
			SetGridBackground ( MyPanel );



			MyPanel.MouseLeftButtonDown+=FrameMoveOnClick;
			MyPanel.MouseLeftButtonUp+=MouseLeftButtonRelease;
			MyPanel.MouseLeave+=OnMouseLeaveGrid;
			MyPanel.PreviewMouseLeftButtonDown+=MoveFrameToMouse;

			//Cursor Name=Frame
			this.PreviewKeyDown+=MoveCursor;
			this.KeyUp+=OnLeftShiftRealease;

			ShiftTracker=new GridLocation { Row=-1 , Column=-1 };
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

		#region Local Modules


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

		private void RenderTransformCursor ( GridLocation finalLocation )
			{
			GridLocation delta = GridLocation.Substraction ( GetActiveCell ( ) , finalLocation );

			GridLocation spanstart;
			GridLocation spannumber;
			if ( finalLocation.Row<GetActiveCell ( ).Row ) { spanstart.Row=finalLocation.Row; } else { spanstart.Row=GetActiveCell ( ).Row; }
			if ( finalLocation.Column<GetActiveCell ( ).Column ) { spanstart.Column=finalLocation.Column; } else { spanstart.Column=GetActiveCell ( ).Column; }

			spannumber.Row=Math.Abs ( delta.Row )+1;
			spannumber.Column=Math.Abs ( delta.Column )+1;

			Grid.SetColumn ( Frame , spanstart.Column );
			Grid.SetRow ( Frame , spanstart.Row );

			Grid.SetColumnSpan ( Frame , spannumber.Column );
			Grid.SetRowSpan ( Frame , spannumber.Row );

			Frame.Focus ( );
			}
		#endregion

		#region Mouse Behaviour
		private delegate void UpdateMousePosition ( GridLocation location );
		private bool MouseLeftClickHolded;
		private void MoveFrameToMouse ( object sender , MouseButtonEventArgs e )
			{
			RenderTransformCursor ( GetActiveCell ( ) );
			GridLocation mouseLocation;
			mouseLocation=GetMouseLocation ( e.GetPosition ( MyPanel ) );
			SetActiveCell ( mouseLocation );
			Frame.Focus ( );
			}
		private GridLocation GetMouseLocation ( Point point )
			{
			int row = 0;
			int col = 0;
			double accumulatedHeight = 0.0;
			double accumulatedWidth = 0.0;
			// calc row mouse was over
			foreach ( var rowDefinition in MyPanel.RowDefinitions )
				{
				accumulatedHeight+=rowDefinition.ActualHeight;
				if ( accumulatedHeight>=point.Y )
					break;
				row++;
				}
			// calc col mouse was over
			foreach ( var columnDefinition in MyPanel.ColumnDefinitions )
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
			MouseLeftClickHolded=true;
			new Thread ( ( ) =>
			{
				Thread.CurrentThread.IsBackground=true;
				MouseLocationLooper ( );
			} ).Start ( );

			}
		private void MouseLeftButtonRelease ( object sender , MouseButtonEventArgs e )
			{
			MouseLeftClickHolded=false;
			}
		private void MouseLocationLooper ( )
			{
			int i = 1;
			do
				{
				Point point;
				MyPanel.Dispatcher.Invoke ( ( ) =>
				{
					point=Mouse.GetPosition ( MyPanel );
				} );
				int row = 0;
				int col = 0;
				double accumulatedHeight = 0.0;
				double accumulatedWidth = 0.0;
				// calc row mouse was over
				foreach ( var rowDefinition in MyPanel.RowDefinitions )
					{
					accumulatedHeight+=rowDefinition.ActualHeight;
					if ( accumulatedHeight>=point.Y )
						break;
					row++;
					}
				// calc col mouse was over
				foreach ( var columnDefinition in MyPanel.ColumnDefinitions )
					{
					accumulatedWidth+=columnDefinition.ActualWidth;
					if ( accumulatedWidth>=point.X )
						break;
					col++;
					}

				GridLocation mouseLocation;
				mouseLocation.Column=col;
				mouseLocation.Row=row;
				Dispatcher.BeginInvoke ( new UpdateMousePosition ( RenderTransformCursor ) , mouseLocation );
				Dispatcher.BeginInvoke ( new UpdateMousePosition ( SelectionPassingToKeyboard ) , mouseLocation );
				if ( i==1 )
					{
					Dispatcher.BeginInvoke ( new UpdateMousePosition ( SetActiveCell ) , mouseLocation );
					}
				i++;

				Thread.Sleep ( 20 );
				} while ( MouseLeftClickHolded );
			}

		private void OnMouseLeaveGrid ( object sender , MouseEventArgs e )
			{
			MouseLeftClickHolded=false;
			}
		#endregion

		#region KeyBoard Behaviour
		private delegate void ShiftDown ( GridLocation location );
		private GridLocation ShiftTracker;
		private void SelectionPassingToKeyboard ( GridLocation FromMouse )
			{
			if ( Grid.GetRowSpan ( Frame )>1||Grid.GetColumnSpan ( Frame )>1 )
				{
				ShiftTracker=FromMouse;
				}
			}
		private void OnLeftShiftRealease ( object sender , KeyEventArgs e )
			{
			if ( Keyboard.IsKeyUp ( Key.LeftShift ) )
				{
				ShiftTracker.Row=-1;
				ShiftTracker.Column=-1;
				}

			}
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
			if ( ShiftTracker.Row==-1||ShiftTracker.Column==-1 ) { ShiftTracker=GetActiveCell ( ); }
			if ( !Keyboard.IsKeyDown ( Key.LeftShift )
				&&( Keyboard.IsKeyDown ( Key.Up )
				||Keyboard.IsKeyDown ( Key.Down )
				||Keyboard.IsKeyDown ( Key.Left )
				||Keyboard.IsKeyDown ( Key.Right )
				) )
				{
				RenderTransformCursor ( GetActiveCell ( ) );
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
					catch ( System.Exception )
						{
						SetActiveCell ( actualLocation );
						}
					finally
						{

						if ( actualLocation.Row>MyPanel.RowDefinitions.Count-1 ) { actualLocation.Row=MyPanel.RowDefinitions.Count-1; }
						if ( actualLocation.Column>MyPanel.ColumnDefinitions.Count-1 ) { actualLocation.Column=MyPanel.ColumnDefinitions.Count-1; }
						SetActiveCell ( actualLocation );
						Frame.Focus ( );
						e.Handled=true;
						}


					}

				}
			if ( Keyboard.IsKeyDown ( Key.LeftShift ) )
				{

				GridLocation actualLocation = ShiftTracker;

				if ( Keyboard.IsKeyDown ( Key.LeftShift )&&Keyboard.IsKeyDown ( Key.Left ) )
					{ actualLocation.Column=actualLocation.Column-1; }
				if ( Keyboard.IsKeyDown ( Key.LeftShift )&&Keyboard.IsKeyDown ( Key.Up ) )
					{ actualLocation.Row=actualLocation.Row-1; }
				if ( Keyboard.IsKeyDown ( Key.LeftShift )&&Keyboard.IsKeyDown ( Key.Right ) )
					{ actualLocation.Column=actualLocation.Column+1; }
				if ( Keyboard.IsKeyDown ( Key.LeftShift )&&Keyboard.IsKeyDown ( Key.Down ) )
					{ actualLocation.Row=actualLocation.Row+1; }

				if ( actualLocation.Row<0 ) { actualLocation.Row=0; }
				if ( actualLocation.Column<0 ) { actualLocation.Column=0; }
				if ( actualLocation.Row>MyPanel.RowDefinitions.Count-1 ) { actualLocation.Row=MyPanel.RowDefinitions.Count-1; }
				if ( actualLocation.Column>MyPanel.ColumnDefinitions.Count-1 ) { actualLocation.Column=MyPanel.ColumnDefinitions.Count-1; }

				ShiftTracker=actualLocation;
				RenderTransformCursor ( ShiftTracker );
				Frame.Focus ( );
				e.Handled=true;
				}

			}
		#endregion




		}



	public class CursorErrorHandler
		{

		}

	}
