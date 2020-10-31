using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace CustomPanelGrid
	{


	public static class UIHelper
		{
		public static T TryFindParent<T> ( this DependencyObject child )
where T : DependencyObject
			{
			//get parent item
			DependencyObject parentObject = GetParentObject ( child );

			//we've reached the end of the tree
			if ( parentObject==null ) return null;

			//check if the parent matches the type we're looking for
			T parent = parentObject as T;
			if ( parent!=null )
				{
				return parent;
				}
			else
				{
				//use recursion to proceed with next level
				return TryFindParent<T> ( parentObject );
				}
			}

		public static DependencyObject GetParentObject ( DependencyObject child )
			{
			if ( child==null ) return null;
			ContentElement contentElement = child as ContentElement;

			if ( contentElement!=null )
				{
				DependencyObject parent = ContentOperations.GetParent ( contentElement );
				if ( parent!=null ) return parent;

				FrameworkContentElement fce = contentElement as FrameworkContentElement;
				return fce!=null ? fce.Parent : null;
				}

			//if it's not a ContentElement, rely on VisualTreeHelper
			return VisualTreeHelper.GetParent ( child );
			}
		}

	/// <summary>
	/// Interaction logic for CursorBox.xaml
	/// </summary>
	public partial class CursorBox : UserControl
		{

		public CursorBox ( )
			{
			InitializeComponent ( );
			//Grid


			try
				{
				var MyPanel = UIHelper.TryFindParent<Grid> ( this );
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

			//CursorBox Name=this
			this.PreviewKeyDown+=MoveCursor;
			this.KeyUp+=OnLeftShiftRealease;

			ShiftTracker=new GridLocation { Row=-1 , Column=-1 };

			}







		#region Parent Grid Setup


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
			Grid.SetColumn ( this , gridLocation.Column );
			Grid.SetRow ( this , gridLocation.Row );
			activeCell=gridLocation;
			}
		private void SetActiveCell ( )
			{
			activeCell.Column=Grid.GetColumn ( this );
			activeCell.Row=Grid.GetRow ( this );
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

			Grid.SetColumn ( this , spanstart.Column );
			Grid.SetRow ( this , spanstart.Row );

			Grid.SetColumnSpan ( this , spannumber.Column );
			Grid.SetRowSpan ( this , spannumber.Row );

			this.Focus ( );
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
			this.Focus ( );
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
			if ( Grid.GetRowSpan ( this )>1||Grid.GetColumnSpan ( this )>1 )
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
					Grid.SetColumn ( this , Grid.GetColumn ( this )-1 );
					break;
				case System.Windows.Input.Key.Up:
					Grid.SetRow ( this , Grid.GetRow ( this )-1 );
					break;
				case System.Windows.Input.Key.Right:
					Grid.SetColumn ( this , Grid.GetColumn ( this )+1 );
					break;
				case System.Windows.Input.Key.Down:
					Grid.SetRow ( this , Grid.GetRow ( this )+1 );
					break;
				}
			actualLocation.Row=Grid.GetRow ( this );
			actualLocation.Column=Grid.GetColumn ( this );
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
						this.Focus ( );
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
				this.Focus ( );
				e.Handled=true;
				}

			}
		#endregion




		}



	public class CursorErrorHandler
		{

		}

	}
