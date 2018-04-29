﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Forms;
using Catel.Collections;
using Catel.Data;
using Catel.MVVM;
using Catel.Services;
using Common.Controls;
using Common.Controls.NameGeneration;
using GongSolutions.Wpf.DragDrop;
using GongSolutions.Wpf.DragDrop.Utilities;
using Vixen.Sys;
using VixenModules.App.CustomPropEditor.Model;
using VixenModules.App.CustomPropEditor.Services;
using DragDropEffects = System.Windows.DragDropEffects;
using IDropTarget = GongSolutions.Wpf.DragDrop.IDropTarget;

namespace VixenModules.App.CustomPropEditor.ViewModels
{
	public class ElementOrderViewModel : ViewModelBase, IDropTarget
	{
		private static NLog.Logger Logging = NLog.LogManager.GetCurrentClassLogger();

		public ElementOrderViewModel(Prop p)
		{
			LeafNodes = new ObservableCollection<ElementModelViewModel>();
			SelectedItems = new ObservableCollection<ElementModelViewModel>();
			Prop = p;
		}

		#region Prop model property

		/// <summary>
		/// Gets or sets the Prop value.
		/// </summary>
		[Model]
		public Prop Prop
		{
			get { return GetValue<Prop>(PropProperty); }
			private set
			{
				SetValue(PropProperty, value);
				RefreshElementLeafViewModels();
			}
		}

		/// <summary>
		/// Prop property data.
		/// </summary>
		public static readonly PropertyData PropProperty = RegisterProperty("Prop", typeof(Prop));

		#endregion

		#region LeafNodes property

		/// <summary>
		/// Gets or sets the LeafNodes value.
		/// </summary>
		public ObservableCollection<ElementModelViewModel> LeafNodes
		{
			get { return GetValue<ObservableCollection<ElementModelViewModel>>(LeafNodesProperty); }
			set { SetValue(LeafNodesProperty, value); }
		}

		/// <summary>
		/// LeafNodes property data.
		/// </summary>
		public static readonly PropertyData LeafNodesProperty = RegisterProperty("LeafNodes", typeof(ObservableCollection<ElementModelViewModel>));

		#endregion

		#region SelectedItems property

		/// <summary>
		/// Gets or sets the SelectedItems value.
		/// </summary>
		public ObservableCollection<ElementModelViewModel> SelectedItems
		{
			get { return GetValue<ObservableCollection<ElementModelViewModel>>(SelectedItemsProperty); }
			set { SetValue(SelectedItemsProperty, value); }
		}

		/// <summary>
		/// SelectedItems property data.
		/// </summary>
		public static readonly PropertyData SelectedItemsProperty = RegisterProperty("SelectedItems", typeof(ObservableCollection<ElementModelViewModel>));

		#endregion

		public void Select(IEnumerable<Guid> modelIds)
		{
			var modelsToSelect = LeafNodes.Where(x => modelIds.Contains(x.ElementModel.Id));
			modelsToSelect.ForEach(x => x.IsSelected = true);
		}

		public void DeselectAll()
		{
			SelectedItems.Clear();
		}


		internal void RefreshElementLeafViewModels()
		{
			LeafNodes.Clear();
			//foreach (var elementModel in PropModelServices.Instance().GetLeafNodes().Where(x => x.IsLightNode).OrderBy(x => x.Order))
			//{
			//	LeafNodes.Add(new ElementModelViewModel(elementModel, null));	
			//}
			LeafNodes.AddRange(ElementModelLookUpService.Instance.GetAllModels().Where(x => x.IsLightNode).DistinctBy(x => x.ElementModel.Id).OrderBy(x => x.ElementModel.Order));
		}

		private void ReOrder()
		{
			int index = 0;
			foreach (var elementModelViewModel in LeafNodes)
			{
				elementModelViewModel.ElementModel.Order = index++;
			}
		}

		#region TemplateRename command

		private Command _templateRenameCommand;

		/// <summary>
		/// Gets the TemplateRename command.
		/// </summary>
		public Command TemplateRenameCommand
		{
			get { return _templateRenameCommand ?? (_templateRenameCommand = new Command(TemplateRename)); }
		}

		/// <summary>
		/// Method to invoke when the TemplateRename command is executed.
		/// </summary>
		private void TemplateRename()
		{
			if (SelectedItems.Count == 1)
			{
				MessageBoxService mbs = new MessageBoxService();
				var result = mbs.GetUserInput("Please enter the new name.", "Rename", SelectedItems[0].ElementModel.Name);
				if (result.Result == MessageResult.OK)
				{
					SelectedItems.First().Name = PropModelServices.Instance().Uniquify(result.Response);
				}
			}
			else
			{
				PatternRenameSelectedItems();
			}
		}

		public bool PatternRenameSelectedItems()
		{
			if (SelectedItems.Count <= 1)
				return false;

			List<string> oldNames = new List<string>(SelectedItems.Select(x => x.ElementModel.Name).ToArray());
			NameGenerator renamer = new NameGenerator(oldNames);
			if (renamer.ShowDialog() == DialogResult.OK)
			{
				for (int i = 0; i < SelectedItems.Count; i++)
				{
					if (i >= renamer.Names.Count)
					{
						Logging.Warn("Bulk renaming elements, and ran out of new names!");
						break;
					}


					SelectedItems[i].Name = PropModelServices.Instance().Uniquify(renamer.Names[i]);
				}

				return true;
			}

			return false;
		}

		#endregion


		#region Implementation of IDropTarget

		/// <inheritdoc />
		public void DragOver(IDropInfo dropInfo)
		{
			dropInfo.Effects = DragDropEffects.Move;
			dropInfo.DropTargetAdorner = DropTargetAdorners.Insert;
		}

		/// <inheritdoc />
		public void Drop(IDropInfo dropInfo)
		{
			if (dropInfo == null || dropInfo.DragInfo == null)
			{
				return;
			}

			var insertIndex = dropInfo.InsertIndex != dropInfo.UnfilteredInsertIndex ? dropInfo.UnfilteredInsertIndex : dropInfo.InsertIndex;

			var itemsControl = dropInfo.VisualTarget as ItemsControl;
			if (itemsControl != null)
			{
				var editableItems = itemsControl.Items as IEditableCollectionView;
				if (editableItems != null)
				{
					var newItemPlaceholderPosition = editableItems.NewItemPlaceholderPosition;
					if (newItemPlaceholderPosition == NewItemPlaceholderPosition.AtBeginning && insertIndex == 0)
					{
						++insertIndex;
					}
					else if (newItemPlaceholderPosition == NewItemPlaceholderPosition.AtEnd && insertIndex == itemsControl.Items.Count)
					{
						--insertIndex;
					}
				}
			}

			var destinationList = dropInfo.TargetCollection.TryGetList();
			var data = ExtractData(dropInfo.Data).OfType<object>().ToList();

			
			var sourceList = dropInfo.DragInfo.SourceCollection.TryGetList();
			if (sourceList != null)
			{
				foreach (var o in data)
				{
					var index = sourceList.IndexOf(o);
					if (index != -1)
					{
						sourceList.RemoveAt(index);
						// so, is the source list the destination list too ?
						if (destinationList != null && Equals(sourceList, destinationList) && index < insertIndex)
						{
							--insertIndex;
						}
					}
				}
			}
			

			if (destinationList != null)
			{
				var objects2Insert = new List<object>();
				
				foreach (var o in data)
				{
					var obj2Insert = o;
					objects2Insert.Add(obj2Insert);
					destinationList.Insert(insertIndex++, obj2Insert);
				}
			}

			ReOrder();
		}

		public static IEnumerable ExtractData(object data)
		{
			if (data is IEnumerable && !(data is string))
			{
				return (IEnumerable)data;
			}
			else
			{
				return Enumerable.Repeat(data, 1);
			}
		}

		#endregion
	}
}