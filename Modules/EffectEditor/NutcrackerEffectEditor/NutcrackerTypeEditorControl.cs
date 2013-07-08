﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Collections;
using System.Globalization;
using System.Resources;
using System.IO;
using Vixen.Module.EffectEditor;
using Vixen.Module.Effect;
using Vixen.Sys;
using VixenModules.Preview.VixenPreview;
using VixenModules.Preview.VixenPreview.Shapes;
using VixenModules.Effect.Nutcracker;
using Common.Controls;

namespace VixenModules.EffectEditor.NutcrackerEffectEditor
{
	public partial class NutcrackerTypeEditorControl : UserControl, IEffectEditorControl
	{
		private NutcrackerData _data = null;
		public NutcrackerEffects effect = new NutcrackerEffects();
		private DisplayItem displayItem = null;

		public NutcrackerTypeEditorControl()
		{
			InitializeComponent();
			NutcrackerDataValue = new NutcrackerData();
		}

		public IEffect TargetEffect { get; set; }

		public NutcrackerData Data
		{
			get { return _data; }
			set { _data = value; }
		}

		public NutcrackerData NutcrackerDataValue
		{
			get { return _data; }
			set { _data = value; }
		}

		public object[] EffectParameterValues
		{
			get
			{
				object[] o = new object[] {NutcrackerDataValue};
				Data = o[0] as NutcrackerData;
				return o;
			}
			set { Data = value[0] as NutcrackerData; }
		}

		private void timerRender_Tick(object sender, EventArgs e)
		{
			effect.RenderNextEffect(Data.CurrentEffect);
			int stringCount = StringCount;
			if (displayItem != null && displayItem.Shape != null) {
				if (displayItem.Shape is PreviewMegaTree) {
					PreviewMegaTree tree = displayItem.Shape as PreviewMegaTree;
					for (int stringNum = 0; stringNum < stringCount; stringNum++) {
						int currentString = stringCount - stringNum - 1;
						PreviewBaseShape treeString = tree._strings[currentString];
						//PreviewBaseShape treeString = tree._strings[stringNum];
						for (int pixelNum = 0; pixelNum < treeString.Pixels.Count; pixelNum++)
						{
							treeString.Pixels[pixelNum].PixelColor = effect.Pixels[stringNum][pixelNum];
						}
					}
				}
				if (displayItem.Shape is PreviewPixelGrid) {
					PreviewPixelGrid grid = displayItem.Shape as PreviewPixelGrid;
					for (int stringNum = 0; stringNum < stringCount; stringNum++) {
						//int currentString = stringCount - stringNum - 1;
						//PreviewBaseShape gridString = grid._strings[currentString];
						PreviewBaseShape gridString = grid._strings[stringNum];
						for (int pixelNum = 0; pixelNum < gridString.Pixels.Count; pixelNum++)
						{
							gridString.Pixels[pixelNum].PixelColor = effect.Pixels[stringNum][pixelNum];
						}
					}
				}
				else if (displayItem.Shape is PreviewArch) {
					PreviewArch arch = displayItem.Shape as PreviewArch;
					for (int pixelNum = 0; pixelNum < arch.PixelCount; pixelNum++) {
						arch.Pixels[pixelNum].PixelColor = effect.Pixels[0][pixelNum];
					}
				}
				else if (displayItem.Shape is PreviewLine) {
					PreviewLine line = displayItem.Shape as PreviewLine;
					for (int pixelNum = 0; pixelNum < line.PixelCount; pixelNum++) {
						line.Pixels[pixelNum].PixelColor = effect.Pixels[0][pixelNum];
					}
				}
			}
			preview.RenderInForeground();
		}

		private void PopulateEffectComboBox()
		{
			foreach (NutcrackerEffects.Effects nutcrackerEffect in Enum.GetValues(typeof(NutcrackerEffects.Effects)))
			{
				comboBoxEffect.Items.Add(nutcrackerEffect.ToString());
			}
		}

		private bool loading = true;
		private void NutcrackerTypeEditorControl_Load(object sender, EventArgs e)
		{
			PopulateEffectComboBox();
			
			//foreach (ElementNode node in Data.TargetNodes) {
			//    if (node != null) {
			//        Console.WriteLine(node.Name);
			//        //RenderNode(node);
			//    }
			//}


			effect.Data = Data;

			// Load item from Data
			SetCurrentEffect(Data.CurrentEffect);
			comboBoxEffect.SelectedItem = Data.CurrentEffect.ToString();
			trackBarSpeed.Value = Data.Speed;

			LoadBarsData();
			LoadButterflyData();
			LoadColorWashData();
			LoadGarlandData();
			LoadFire();
			LoadLife();
			LoadMeteor();
			LoadFireworks();
			LoadSnowflakes();
			LoadSnowstorm();
			LoadSpirals();
			LoadTwinkles();
			LoadText();
			LoadPicture();
			LoadSpirograph();
			LoadTree();
			LoadMovie();
			LoadPictureTile();
			LoadColors();
			LoadPreview();

			scrollPixelSize.Value = Data.PixelSize;

			timerRender.Start();

			loading = false;
		}

		private void LoadColors()
		{
			for (int colorNum = 0; colorNum < effect.Palette.Count(); colorNum++) {
				Color color = effect.Palette.Colors[colorNum];
				CheckBox checkBox =
					this.Controls.Find("checkBoxColor" + (colorNum + 1).ToString(), true).FirstOrDefault() as CheckBox;
				Panel colorPanel = this.Controls.Find("panelColor" + (colorNum + 1).ToString(), true).FirstOrDefault() as Panel;
				checkBox.Checked = true;
				colorPanel.BackColor = color;
			}
		}

		private void SetCurrentEffect(NutcrackerEffects.Effects selectedEffect)
		{
			Data.CurrentEffect = selectedEffect;
			effect.SetNextState(true);
			SetCurrentTab(selectedEffect.ToString());
		}

		private void SetCurrentEffect(string effectName)
		{
			foreach (NutcrackerEffects.Effects nutcrackerEffect in Enum.GetValues(typeof (NutcrackerEffects.Effects))) {
				if (nutcrackerEffect.ToString() == effectName) {
					SetCurrentEffect(nutcrackerEffect);
				}
			}
		}

		private void SetCurrentTab(string tabName)
		{
			foreach (TabPage tab in tabEffectProperties.TabPages) {
				if (tab.Text == tabName) {
					tabEffectProperties.SelectedTab = tab;
					break;
				}
			}
		}

		private void trackBarSpeed_ValueChanged(object sender, EventArgs e)
		{
			Data.Speed = trackBarSpeed.Value;
		}

		private void comboBoxEffect_SelectedIndexChanged(object sender, EventArgs e)
		{
			//if (loading) return;
			effect.SetNextState(true);

			SetCurrentEffect(comboBoxEffect.SelectedItem.ToString());
		}

		private void DeletePreviewDisplayItem()
		{
			if (preview.DisplayItems != null && preview.DisplayItems.Count > 0) {
				preview.DisplayItems.RemoveAt(0);
			}
		}

		private int StringCount
		{
			get
			{
				int childCount = 0;
				foreach (ElementNode node in TargetEffect.TargetNodes.FirstOrDefault().Children) {
					if (!node.IsLeaf) {
						childCount++;
					}
				}
				if (childCount == 0 && TargetEffect.TargetNodes.FirstOrDefault().Children.Count() > 0) {
					childCount = 1;
				}
				return childCount;
			}
		}

		private int PixelsPerString()
		{
			int pps = PixelsPerString(TargetEffect.TargetNodes.FirstOrDefault());
			return pps;
		}

		private int PixelsPerString(ElementNode parentNode)
		{
			int pps = 0;
			int leafCount = 0;
			int groupCount = 0;
			foreach (ElementNode node in parentNode.Children) {
				if (node.IsLeaf) {
					leafCount++;
				}
				else {
					groupCount++;
				}
			}
			if (groupCount == 0) {
				pps = leafCount;
			}
			else {
				pps = PixelsPerString(parentNode.Children.FirstOrDefault());
			}
			return pps;
		}

		#region Preview

		private void SetupMegaTree(int degrees)
		{
			int stringCount = StringCount;
			if (stringCount < 2) return;
			preview.Data = new VixenPreviewData();
			preview.LoadBackground();
			preview.BackgroundAlpha = 0;
			displayItem = new DisplayItem();
			PreviewMegaTree tree = new PreviewMegaTree(new PreviewPoint(10, 10), null);
			tree.BaseHeight = 25;
			tree.TopHeight = 1;
			tree.TopWidth = 1;
			tree.StringType = PreviewBaseShape.StringTypes.Pixel;
			tree.Degrees = degrees;

			if (degrees == 90)
				tree.StringCount = stringCount*4;
			if (degrees == 180)
				tree.StringCount = stringCount*2;
			if (degrees == 270)
				tree.StringCount = (int) (stringCount*1.25);
			if (degrees == 360)
				tree.StringCount = stringCount;

			//Console.WriteLine("degrees:" + degrees + " StringCount:" + stringCount + " tree.StringCount:" + tree.StringCount);

			tree.PixelCount = PixelsPerString();
			tree.PixelSize = Data.PixelSize;
			tree.PixelColor = Color.White;
			tree.Top = 10;
			tree.Left = 10;
			tree.BottomRight.X = preview.Width - 10;
			tree.BottomRight.Y = preview.Height - 10;
			tree.Layout();
			displayItem.Shape = tree;

			preview.AddDisplayItem(displayItem);
		}

		private void SetupArch()
		{
			preview.Data = new VixenPreviewData();
			preview.LoadBackground();
			preview.BackgroundAlpha = 0;
			displayItem = new DisplayItem();
			PreviewArch arch = new PreviewArch(new PreviewPoint(10, 10), null);

			arch.PixelCount = PixelsPerString();
			arch.PixelSize = Data.PixelSize;
			arch.PixelColor = Color.White;
			arch.TopLeft = new Point(10, preview.Height/2);
			arch.BottomRight = new Point((int) (preview.Width - 10), (int) (preview.Height - 10));
			arch.Layout();
			displayItem.Shape = arch;

			preview.AddDisplayItem(displayItem);
		}

		private void SetupLine(bool horizontal)
		{
			preview.Data = new VixenPreviewData();
			preview.LoadBackground();
			preview.BackgroundAlpha = 0;
			displayItem = new DisplayItem();
			PreviewPoint p1, p2;
			if (horizontal) {
				p1 = new PreviewPoint(10, preview.Height/2);
				p2 = new PreviewPoint(preview.Width - 10, preview.Height/2);
			}
			else {
				p1 = new PreviewPoint(preview.Width/2, preview.Height - 10);
				p2 = new PreviewPoint(preview.Width/2, 10);
			}
			PreviewLine line = new PreviewLine(p1, p2, PixelsPerString(), null);

			line.PixelCount = PixelsPerString();
			line.PixelSize = Data.PixelSize;
			line.PixelColor = Color.White;
			line.Layout();
			displayItem.Shape = line;

			preview.AddDisplayItem(displayItem);
		}

		private void SetupPixelGrid()
		{
			if (StringCount < 2) return;
			preview.Data = new VixenPreviewData();
			preview.LoadBackground();
			preview.BackgroundAlpha = 0;
			displayItem = new DisplayItem();
			PreviewPixelGrid grid = new PreviewPixelGrid(new PreviewPoint(10, 10), null);
			grid.StringType = PreviewBaseShape.StringTypes.Pixel;
			grid.StringCount = StringCount;
			grid.LightsPerString = PixelsPerString();
			//tree.PixelCount = PixelsPerString();
			grid.PixelSize = Data.PixelSize;
			grid.PixelColor = Color.White;
			grid.Top = 10;
			grid.Left = 10;
			grid.BottomRight.X = preview.Width - 10;
			grid.BottomRight.Y = preview.Height - 10;
			grid.Layout();
			displayItem.Shape = grid;

			preview.AddDisplayItem(displayItem);
		}

		public void SetupPreview()
		{
			DeletePreviewDisplayItem();

			effect.InitBuffer(StringCount, PixelsPerString());

			switch (Data.PreviewType) {
				case NutcrackerEffects.PreviewType.Tree90:
					SetupMegaTree(90);
					break;
				case NutcrackerEffects.PreviewType.Tree180:
					SetupMegaTree(180);
					break;
				case NutcrackerEffects.PreviewType.Tree270:
					SetupMegaTree(270);
					break;
				case NutcrackerEffects.PreviewType.Tree360:
					SetupMegaTree(360);
					break;
				case NutcrackerEffects.PreviewType.Grid:
					SetupPixelGrid();
					break;
				case NutcrackerEffects.PreviewType.Arch:
					SetupArch();
					break;
				case NutcrackerEffects.PreviewType.HorizontalLine:
					SetupLine(true);
					break;
				case NutcrackerEffects.PreviewType.VerticalLine:
					SetupLine(false);
					break;
				default:
					SetupMegaTree(180);
					break;
			}
		}

		private void comboBoxDisplayType_SelectedIndexChanged(object sender, EventArgs e)
		{
			foreach (NutcrackerEffects.PreviewType previewType in Enum.GetValues(typeof (NutcrackerEffects.PreviewType))) {
				if (previewType.ToString() == comboBoxDisplayType.SelectedItem.ToString()) {
					Data.PreviewType = previewType;
					break;
				}
			}
			SetupPreview();
		}

		private void LoadPreview()
		{
			foreach (NutcrackerEffects.PreviewType previewType in Enum.GetValues(typeof (NutcrackerEffects.PreviewType))) {
				comboBoxDisplayType.Items.Add(previewType.ToString());
			}

			comboBoxDisplayType.SelectedItem = Data.PreviewType.ToString();

			SetupPreview();
		}

		#endregion // Preview

		#region Colors

		private void checkBoxColor_CheckedChanged(object sender, EventArgs e)
		{
			if (loading) return;
			CheckBox checkBox = sender as CheckBox;
			string colorNum = checkBox.Name.Substring(checkBox.Name.Length - 1);
			if (checkBox.Checked) {
				string panelName = "panelColor" + colorNum;
				Panel colorPanel = this.Controls.Find(panelName, true).FirstOrDefault() as Panel;
				effect.Palette.SetColor(Convert.ToInt32(colorNum), colorPanel.BackColor);
			}
			else {
				effect.Palette.ColorsActive[Convert.ToInt32(colorNum) - 1] = false;
			}
			effect.SetNextState(true);
		}

		private void panelColor_Click(object sender, EventArgs e)
		{
			Panel colorPanel = sender as Panel;
			colorDialog.Color = colorPanel.BackColor;
			if (colorDialog.ShowDialog() == DialogResult.OK) {
				colorPanel.BackColor = colorDialog.Color;
				string colorNum = colorPanel.Name.Substring(colorPanel.Name.Length - 1);
				effect.Palette.SetColor(Convert.ToInt32(colorNum), colorPanel.BackColor, false);
			}
		}

		#endregion

		#region Bars

		private void LoadBarsData()
		{
			trackBarPaletteRepeat.Value = Data.Bars_PaletteRepeat;
			checkBoxBars3D.Checked = Data.Bars_3D;
			checkBoxBarsHighlight.Checked = Data.Bars_Highlight;
			switch (Data.Bars_Direction) {
				case 0:
					comboBoxBarsDirection.SelectedItem = "Up";
					break;
				case 1:
					comboBoxBarsDirection.SelectedItem = "Down";
					break;
				case 2:
					comboBoxBarsDirection.SelectedItem = "Expand";
					break;
				case 3:
					comboBoxBarsDirection.SelectedItem = "Compress";
					break;
			}
		}

		private void Bars_ParametersChanged(object sender, EventArgs e)
		{
			if (loading) return;
			Data.Bars_PaletteRepeat = trackBarPaletteRepeat.Value;
			Data.Bars_Highlight = checkBoxBarsHighlight.Checked;
			Data.Bars_3D = checkBoxBars3D.Checked;
			if (comboBoxBarsDirection.SelectedItem != null) {
				switch (comboBoxBarsDirection.SelectedItem.ToString()) {
					case "Up":
						Data.Bars_Direction = 0;
						break;
					case "Down":
						Data.Bars_Direction = 1;
						break;
					case "Expand":
						Data.Bars_Direction = 2;
						break;
					case "Compress":
						Data.Bars_Direction = 3;
						break;
				}
			}
		}

		private void trackBarPaletteRepeat_ValueChanged(Common.Controls.ControlsEx.ValueControls.ValueControl sender,
		                                                Common.Controls.ControlsEx.ValueControls.ValueChangedEventArgs e)
		{
			Bars_ParametersChanged(sender, EventArgs.Empty);
		}

		#endregion

		#region Butterfly

		private void LoadButterflyData()
		{
			trackButterflyStyle.Value = Data.Butterfly_Style;
			trackButterflyBkgrdChunks.Value = Data.Butterfly_BkgrdChunks;
			trackButterflyBkgrdSkip.Value = Data.Butterfly_Style;
			switch (Data.Butterfly_Colors) {
				case 0:
					comboBoxButterflyColors.SelectedItem = "Rainbow";
					break;
				case 1:
					comboBoxButterflyColors.SelectedItem = "Palette";
					break;
			}
		}

		private void Butterfly_ValueChanged(Common.Controls.ControlsEx.ValueControls.ValueControl sender,
		                                    Common.Controls.ControlsEx.ValueControls.ValueChangedEventArgs e)
		{
			if (loading) return;
			Data.Butterfly_Style = trackButterflyStyle.Value;
			Data.Butterfly_BkgrdChunks = trackButterflyBkgrdChunks.Value;
			Data.Butterfly_BkgrdSkip = trackButterflyBkgrdSkip.Value;
		}

		private void comboBoxButterflyColors_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (loading) return;
			switch (comboBoxButterflyColors.SelectedItem.ToString()) {
				case "Rainbow":
					Data.Butterfly_Colors = 0;
					break;
				case "Palette":
					Data.Butterfly_Colors = 1;
					break;
			}
		}

		#endregion

		#region ColorWash

		private void LoadColorWashData()
		{
			trackColorWashCount.Value = Data.ColorWash_Count;
			checkBoxColorWashHorizontalFade.Checked = Data.ColorWash_FadeHorizontal;
			checkBoxColorWashVerticalFade.Checked = Data.ColorWash_FadeVertical;
		}

		private void trackColorWashCount_ValueChanged(Common.Controls.ControlsEx.ValueControls.ValueControl sender,
		                                              Common.Controls.ControlsEx.ValueControls.ValueChangedEventArgs e)
		{
			Data.ColorWash_Count = trackColorWashCount.Value;
			effect.SetNextState(true);
		}

		private void checkBoxColorWashHorizontalFade_CheckedChanged(object sender, EventArgs e)
		{
			Data.ColorWash_FadeHorizontal = checkBoxColorWashHorizontalFade.Checked;
			effect.SetNextState(true);
		}

		private void checkBoxColorWashVerticalFade_CheckedChanged(object sender, EventArgs e)
		{
			Data.ColorWash_FadeVertical = checkBoxColorWashVerticalFade.Checked;
			effect.SetNextState(true);
		}

		#endregion

		#region Fire

		private void LoadFire()
		{
			trackFireHeight.Value = Data.Fire_Height;
		}

		private void trackFireHeight_ValueChanged(Common.Controls.ControlsEx.ValueControls.ValueControl sender,
		                                          Common.Controls.ControlsEx.ValueControls.ValueChangedEventArgs e)
		{
			Data.Fire_Height = trackFireHeight.Value;
		}

		#endregion

		#region Garlands

		private void LoadGarlandData()
		{
			trackBarGarlandSpacing.Value = Data.Garland_Spacing;
			trackBarGarlandType.Value = Data.Garland_Type;
		}

		private void Garlands_ValueChanged(Common.Controls.ControlsEx.ValueControls.ValueControl sender,
		                                   Common.Controls.ControlsEx.ValueControls.ValueChangedEventArgs e)
		{
			if (loading) return;
			Data.Garland_Type = trackBarGarlandType.Value;
			Data.Garland_Spacing = trackBarGarlandSpacing.Value;
			effect.SetNextState(true);
		}

		#endregion

		#region Life

		private void LoadLife()
		{
			trackLifeType.Value = Data.Life_Type;
			trackLifeCellsToStart.Value = Data.Life_CellsToStart;
		}

		private void Life_ValueChanged(Common.Controls.ControlsEx.ValueControls.ValueControl sender,
		                               Common.Controls.ControlsEx.ValueControls.ValueChangedEventArgs e)
		{
			if (loading) return;
			Data.Life_CellsToStart = trackLifeCellsToStart.Value;
			Data.Life_Type = trackLifeType.Value;
			effect.SetNextState(true);
		}

		#endregion // Life

		#region Meteor

		private void LoadMeteor()
		{
			comboBoxMeteorColors.SelectedIndex = Data.Meteor_Colors;
			trackMeteorCount.Value = Data.Meteor_Count;
			trackMeteorTrailLength.Value = Data.Meteor_TrailLength;
		}

		private void comboBoxMeteorColors_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (loading) return;
			switch (comboBoxMeteorColors.SelectedItem.ToString()) {
				case "Rainbow":
					Data.Meteor_Colors = 0;
					break;
				case "Range":
					Data.Meteor_Colors = 1;
					break;
				case "Palette":
					Data.Meteor_Colors = 2;
					break;
			}
		}

		private void Meteor_ValueChanged(Common.Controls.ControlsEx.ValueControls.ValueControl sender,
		                                 Common.Controls.ControlsEx.ValueControls.ValueChangedEventArgs e)
		{
			if (loading) return;
			Data.Meteor_Count = trackMeteorCount.Value;
			Data.Meteor_TrailLength = trackMeteorTrailLength.Value;
		}

		#endregion // Meteor

		#region Fireworks

		private void LoadFireworks()
		{
			trackFireworkNumberOfExplosions.Value = Data.Fireworks_Explosions;
			trackFireworkFade.Value = Data.Fireworks_Fade;
			trackFireworkParticles.Value = Data.Fireworks_Particles;
			trackerFireworkVelocity.Value = Data.Fireworks_Velocity;
		}

		private void Fireworks_ValueChanged(Common.Controls.ControlsEx.ValueControls.ValueControl sender,
		                                    Common.Controls.ControlsEx.ValueControls.ValueChangedEventArgs e)
		{
			if (loading) return;
			Data.Fireworks_Explosions = trackFireworkNumberOfExplosions.Value;
			Data.Fireworks_Fade = trackFireworkFade.Value;
			Data.Fireworks_Particles = trackFireworkParticles.Value;
			Data.Fireworks_Velocity = trackerFireworkVelocity.Value;
		}

		#endregion

		#region Snowflakes

		private void LoadSnowflakes()
		{
			trackSnowflakeMax.Value = Data.Snowflakes_Max;
			trackSnowflakeType.Value = Data.Snowflakes_Type;
		}

		private void Snowflake_ValueChanged(Common.Controls.ControlsEx.ValueControls.ValueControl sender,
		                                    Common.Controls.ControlsEx.ValueControls.ValueChangedEventArgs e)
		{
			if (loading) return;
			Data.Snowflakes_Max = trackSnowflakeMax.Value;
			Data.Snowflakes_Type = trackSnowflakeType.Value;
		}

		#endregion // Snowflakes

		#region Snowstorm

		private void LoadSnowstorm()
		{
			trackSnowstormMaxFlakes.Value = Data.Snowstorm_MaxFlakes;
			trackSnowstormTrailLength.Value = Data.Snowstorm_TrailLength;
		}

		private void Snowstorm_ValueChanged(Common.Controls.ControlsEx.ValueControls.ValueControl sender,
		                                    Common.Controls.ControlsEx.ValueControls.ValueChangedEventArgs e)
		{
			if (loading) return;
			Data.Snowstorm_TrailLength = trackSnowstormTrailLength.Value;
			Data.Snowstorm_MaxFlakes = trackSnowstormMaxFlakes.Value;
		}

		#endregion // Snowstorm

		#region Spirals

		private void LoadSpirals()
		{
			trackSpiralsDirection.Value = Data.Spirals_Direction;
			trackSpiralsRepeat.Value = Data.Spirals_PaletteRepeat;
			trackSpiralsRotations.Value = Data.Spirals_Rotation;
			trackSpiralsThickness.Value = Data.Spirals_Thickness;
			checkSpirals3D.Checked = Data.Spirals_3D;
			checkSpiralsBlend.Checked = Data.Spirals_Blend;
		}

		private void Spirals_ValueChanged(Common.Controls.ControlsEx.ValueControls.ValueControl sender,
		                                  Common.Controls.ControlsEx.ValueControls.ValueChangedEventArgs e)
		{
			if (loading) return;
			Data.Spirals_Direction = trackSpiralsDirection.Value;
			Data.Spirals_PaletteRepeat = trackSpiralsRepeat.Value;
			Data.Spirals_Rotation = trackSpiralsRotations.Value;
			Data.Spirals_Thickness = trackSpiralsThickness.Value;
		}

		private void Spirals_CheckedChanged(object sender, EventArgs e)
		{
			if (loading) return;
			Data.Spirals_3D = checkSpirals3D.Checked;
			Data.Spirals_Blend = checkSpiralsBlend.Checked;
		}

		#endregion // Twinkles

		#region Twinkles

		private void LoadTwinkles()
		{
			trackTwinkleCount.Value = Data.Twinkles_Count;
		}

		private void trackTwinkleCount_ValueChanged(Common.Controls.ControlsEx.ValueControls.ValueControl sender,
		                                            Common.Controls.ControlsEx.ValueControls.ValueChangedEventArgs e)
		{
			if (loading) return;
			Data.Twinkles_Count = trackTwinkleCount.Value;
		}

		#endregion // Twinkles

		#region Text

		private void LoadText()
		{
			textTextLine1.Text = Data.Text_Line1;
			textTextLine2.Text = Data.Text_Line2;
			comboBoxTextDirection.SelectedIndex = Data.Text_Direction;
			trackTextTop.Value = Data.Text_Top;
		}

		private void Text_TextChanged(object sender, EventArgs e)
		{
			if (loading) return;
			Data.Text_Line1 = textTextLine1.Text;
			Data.Text_Line2 = textTextLine2.Text;
			Data.Text_Direction = comboBoxTextDirection.SelectedIndex;
			//Data.Text_TextRotation = 
			//Data.Text_Left = 
		}

		private void trackTextTop_ValueChanged(Common.Controls.ControlsEx.ValueControls.ValueControl sender,
		                                       Common.Controls.ControlsEx.ValueControls.ValueChangedEventArgs e)
		{
			if (loading) return;
			Data.Text_Top = trackTextTop.Value;
		}

		private void buttonTextFont_Click(object sender, EventArgs e)
		{
			fontDialog.Font = Data.Text_Font;
			if (fontDialog.ShowDialog() == DialogResult.OK) {
				Data.Text_Font = fontDialog.Font;
			}
		}

		#endregion // Text

		#region Picture

		private void LoadPicture()
		{
			textPictureFileName.Text = Data.Picture_FileName;
			comboBoxPictureDirection.SelectedIndex = Data.Picture_Direction;
			trackPictureGifSpeed.Value = Data.Picture_GifSpeed;
		}

		private void buttonPictureSelect_Click(object sender, EventArgs e)
		{
			fileDialog.Filter = "All Files|*.*|jpg|*.jpg|jpeg|*.jpeg|gif|.gif|png|*.png|bmp|*.bmp";
			if (fileDialog.ShowDialog() == DialogResult.OK) {
				// Copy the file to the Vixen folder
				var imageFile = new System.IO.FileInfo(fileDialog.FileName);
				var destFileName = System.IO.Path.Combine(NutcrackerDescriptor.ModulePath, imageFile.Name);
				var sourceFileName = imageFile.FullName;
				if (sourceFileName != destFileName) {
					System.IO.File.Copy(sourceFileName, destFileName, true);
				}

				textPictureFileName.Text = destFileName;
				Data.Picture_FileName = destFileName;
			}
		}

		private void comboBoxPictureDirection_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (loading) return;
			Data.Picture_Direction = comboBoxPictureDirection.SelectedIndex;
		}

		private void trackPictureGifSpeed_ValueChanged(Common.Controls.ControlsEx.ValueControls.ValueControl sender,
		                                               Common.Controls.ControlsEx.ValueControls.ValueChangedEventArgs e)
		{
			if (loading) return;
			Data.Picture_GifSpeed = trackPictureGifSpeed.Value;
		}

		#endregion // Picture

		#region Spirograph

		private void LoadSpirograph()
		{
			trackSpirographROuter.Value = Data.Spirograph_ROuter;
			trackSpirographRInner.Value = Data.Spirograph_RInner;
			trackSpirographDistance.Value = Data.Spirograph_Distance;
			checkBoxSpirographAnimate.Checked = Data.Spirograph_Animate;
		}

		private void checkBoxSpirographAnimate_CheckedChanged(object sender, EventArgs e)
		{
			if (loading) return;
			Data.Spirograph_Animate = checkBoxSpirographAnimate.Checked;
		}

		private void Spirograph_ValueChanged(Common.Controls.ControlsEx.ValueControls.ValueControl sender,
		                                     Common.Controls.ControlsEx.ValueControls.ValueChangedEventArgs e)
		{
			if (loading) return;
			Data.Spirograph_Distance = trackSpirographDistance.Value;
			Data.Spirograph_ROuter = trackSpirographROuter.Value;
			Data.Spirograph_RInner = trackSpirographRInner.Value;
		}

		#endregion

		#region Tree

		private void LoadTree()
		{
			trackTreeBranches.Value = Data.Tree_Branches;
		}

		private void trackTreeBranches_ValueChanged(Common.Controls.ControlsEx.ValueControls.ValueControl sender,
		                                            Common.Controls.ControlsEx.ValueControls.ValueChangedEventArgs e)
		{
			if (loading) return;
			Data.Tree_Branches = trackTreeBranches.Value;
		}

		#endregion //Tree

		#region Movie

		private void LoadMovie()
		{
			trackMoviePlaybackSpeed.Value = Data.Movie_PlaybackSpeed;
			comboBoxMovieMovementDirection.SelectedIndex = Data.Movie_MovementDirection;
		}

		private void DeleteExistingMovieFiles(string folder)
		{
			System.IO.DirectoryInfo folderInfo = new System.IO.DirectoryInfo(folder);

			foreach (System.IO.FileInfo file in folderInfo.GetFiles()) {
				file.Delete();
			}
			foreach (System.IO.DirectoryInfo dir in folderInfo.GetDirectories()) {
				dir.Delete(true);
			}
		}

		private void ProcessMovie(string movieFileName, string destinationFolder)
		{
			try {
				NutcrackerProcessingMovie f = new NutcrackerProcessingMovie();
				f.Show();
				ffmpeg.ffmpeg converter = new ffmpeg.ffmpeg(movieFileName);
				converter.MakeThumbnails(50, 50, destinationFolder);
				f.Close();
			}
			catch (Exception ex) {
				MessageBox.Show("There was a problem converting " + movieFileName + ": " + ex.Message, "Error Converting Movie",
				                MessageBoxButtons.OKCancel, MessageBoxIcon.Error);
			}
		}

		private void buttonMovieSelectFile_Click(object sender, EventArgs e)
		{
			fileDialog.Filter = "All Files|*.*";
			if (fileDialog.ShowDialog() == DialogResult.OK) {
				// If this effect doesn't have working folder make one.
				// TODO: delete the folder if the effect is removed from the timeline?
				if (Data.Movie_DataPath.Length == 0) {
					Data.Movie_DataPath = Guid.NewGuid().ToString();
				}
				var destFolder = System.IO.Path.Combine(NutcrackerDescriptor.ModulePath, Data.Movie_DataPath);
				if (!System.IO.Directory.Exists(destFolder)) {
					System.IO.Directory.CreateDirectory(destFolder);
				}
				DeleteExistingMovieFiles(destFolder);
				ProcessMovie(fileDialog.FileName, destFolder);
				effect.SetNextState(true);
			}
		}

		private void Movie_ValueChanged(Common.Controls.ControlsEx.ValueControls.ValueControl sender,
		                                Common.Controls.ControlsEx.ValueControls.ValueChangedEventArgs e)
		{
			if (loading) return;
			Data.Movie_PlaybackSpeed = trackMoviePlaybackSpeed.Value;
		}

		private void comboBoxMovieMovementDirection_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (loading) return;
			Data.Movie_MovementDirection = comboBoxMovieMovementDirection.SelectedIndex;
		}

		#endregion // Movies

		#region PictureTile

		//private void LoadPictureTile()
		//{
		//    string folder = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Modules\\Effect\\PictureTiles");
		//    //System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Modules\\Effect\\PictureTiles";
		//    Console.WriteLine(folder);
		//    System.IO.DirectoryInfo folderInfo = new System.IO.DirectoryInfo(folder);

		//    foreach (System.IO.FileInfo file in folderInfo.GetFiles()) {
		//        // TODO: check for valid image formats
		//        if (file.Extension.ToLower() != ".db") {
		//            string title = file.Name;
		//            PictureComboBoxItem item = new PictureComboBoxItem(title, file, comboBoxPictureTileFileName.ItemHeight,
		//                                                               comboBoxPictureTileFileName.ItemHeight);
		//            comboBoxPictureTileFileName.Items.Add(item);

		//            if (item.File.FullName == Data.PictureTile_FileName) {
		//                comboBoxPictureTileFileName.SelectedIndex = comboBoxPictureTileFileName.Items.Count - 1;
		//            }
		//        }
		//    }

		//    if (comboBoxPictureTileFileName.Items.Count > 0 && comboBoxPictureTileFileName.SelectedIndex < 0)
		//        comboBoxPictureTileFileName.SelectedIndex = 0;

		//    trackPictureTileMovementDirection.Value = Data.PictureTile_Direction;
		//    numericPictureTileScale.Value = Convert.ToDecimal(Data.PictureTile_Scaling);
		//    checkPictureTileReplaceColor.Checked = Data.PictureTile_ReplaceColor;
		//    checkPictureTileCopySaturation.Checked = Data.PictureTile_UseSaturation;
		//}

		private const string IMAGE_RESX_SOURCE = "VixenModules.Effect.Nutcracker.PictureTiles";
		private void LoadPictureTile()
		{
			string[] resourceNames = typeof(Nutcracker).Assembly.GetManifestResourceNames();
			foreach (var res in resourceNames)
			{
				string title = res.Replace(IMAGE_RESX_SOURCE + ".", string.Empty); ;
				PictureComboBoxItem item = new PictureComboBoxItem(title, res, comboBoxPictureTileFileName.ItemHeight,
																									comboBoxPictureTileFileName.ItemHeight, typeof(Nutcracker));
				comboBoxPictureTileFileName.Items.Add(item);
				//if (!Data.PictureFile_Custom && item.ResourceName == Data.PictureTile_FileName)
				if (item.ResourceName == Data.PictureTile_FileName)
				{
					comboBoxPictureTileFileName.SelectedIndex = comboBoxPictureTileFileName.Items.Count - 1;
				}

			}
			if (comboBoxPictureTileFileName.Items.Count > 0 && comboBoxPictureTileFileName.SelectedIndex < 0)
				comboBoxPictureTileFileName.SelectedIndex = 0;
			trackPictureTileMovementDirection.Value = Data.PictureTile_Direction;
			numericPictureTileScale.Value = Convert.ToDecimal(Data.PictureTile_Scaling);
			checkPictureTileReplaceColor.Checked = Data.PictureTile_ReplaceColor;
			checkPictureTileCopySaturation.Checked = Data.PictureTile_UseSaturation;
		}

		private void comboBoxPictureTileFileName_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (loading) return;
			PictureComboBoxItem item = comboBoxPictureTileFileName.SelectedItem as PictureComboBoxItem;
			if (item != null) {
				//FileInfo file = item.File;
				//Data.PictureTile_FileName = file.FullName;
				Data.PictureTile_FileName = item.ResourceName;
			}
			effect.SetNextState(true);
		}

		private void PictureTile_ValueChanged(Common.Controls.ControlsEx.ValueControls.ValueControl sender,
		                                      Common.Controls.ControlsEx.ValueControls.ValueChangedEventArgs e)
		{
			if (loading) return;
			Data.PictureTile_Direction = trackPictureTileMovementDirection.Value;
		}

		private void numericPictureTileScale_ValueChanged(object sender, EventArgs e)
		{
			if (loading) return;
			Data.PictureTile_Scaling = Convert.ToDouble(numericPictureTileScale.Value);
			effect.SetNextState(true);
		}

		private void PictureTile_CheckedChanged(object sender, EventArgs e)
		{
			if (loading) return;
			Data.PictureTile_ReplaceColor = checkPictureTileReplaceColor.Checked;
			Data.PictureTile_UseSaturation = checkPictureTileCopySaturation.Checked;
			effect.SetNextState(true);
		}

		private void buttonPictureTileSelect_Click(object sender, EventArgs e)
		{
			fileDialog.Filter = "All Files|*.*|jpg|*.jpg|jpeg|*.jpeg|gif|.gif|png|*.png|bmp|*.bmp";
			if (fileDialog.ShowDialog() == DialogResult.OK) {
				// Copy the file to the Vixen folder
				var imageFile = new System.IO.FileInfo(fileDialog.FileName);
				var destFileName = System.IO.Path.Combine(NutcrackerDescriptor.ModulePath, imageFile.Name);
				var sourceFileName = imageFile.FullName;
				if (sourceFileName != destFileName) {
					System.IO.File.Copy(sourceFileName, destFileName, true);
				}

				textPictureTileFileName.Text = destFileName;
				Data.PictureTile_FileName = destFileName;
			}
		}

		#endregion // PictureTile

		private void buttonHelp_Click(object sender, EventArgs e)
		{
			System.Diagnostics.Process.Start(buttonHelp.Tag.ToString());
		}

		private void scrollPixelSize_ValueChanged(Common.Controls.ControlsEx.ValueControls.ValueControl sender,
		                                          Common.Controls.ControlsEx.ValueControls.ValueChangedEventArgs e)
		{
			Data.PixelSize = scrollPixelSize.Value;
			displayItem.Shape.PixelSize = Data.PixelSize;
		}

	}
}