﻿using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Vixen.Data.Value;
using Vixen.Intent;
using Vixen.Sys;

namespace Vixen.Data.StateCombinator
{
	public class LayeredStateCombinator:StateCombinator<LayeredStateCombinator>
	{
		private readonly IIntentStates _combinedStates = new IntentStateList(4);
		private readonly Dictionary<int, IIntentState<DiscreteValue>> _combinedDiscreteStates = new Dictionary<int, IIntentState<DiscreteValue>>(4);
		private readonly StaticIntentState<LightingValue> _mixedIntentState = new StaticIntentState<LightingValue>(new LightingValue(Color.Black));
		private byte _layer = 0;
		
		public override List<IIntentState> Combine(List<IIntentState> states)
		{
			StateCombinatorValue.Clear();
			if (states == null || states.Count <= 0) return StateCombinatorValue;
			
			_combinedStates.Clear();
			_combinedDiscreteStates.Clear();
			
			//var layer = states.Max(x => x.Layer);
			_layer = 0;
			foreach (var intentState in states)
			{
				if (intentState.Layer > _layer)
				{
					_layer = intentState.Layer;
				}
			}
			if (states.Count > 1)
			{
				foreach (var intentState in states.OrderByDescending(x => x.Layer))
				{
					intentState.Dispatch(this);
				}
			}
			else
			{
				foreach (var intentState in states)
				{
					intentState.Dispatch(this);
				}
			}
			
			if (_combinedStates.Count > 0)
			{
				var color = IntentHelpers.GetOpaqueRGBMaxColorForIntents(_combinedStates);
				_mixedIntentState.SetValue(new LightingValue(color));
				StateCombinatorValue.Add(_mixedIntentState);
			}

			if (_combinedDiscreteStates.Count > 0)
			{
				StateCombinatorValue.AddRange(_combinedDiscreteStates.Values);
			}

			return StateCombinatorValue;
		}

		public override void Handle(IIntentState<LightingValue> obj)
		{
			if (obj.GetValue().Intensity > 0)
			{
				if (obj.Layer == _layer || _combinedStates.Count == 0)
				{
					_combinedStates.AddIntentState(obj);
				}
			}
		}

		public override void Handle(IIntentState<RGBValue> obj)
		{
			if (obj.GetValue().Intensity > 0)
			{
				if (obj.Layer == _layer || _combinedStates.Count == 0)
				{
					_combinedStates.AddIntentState(obj);
				}
			}
		}

		public override void Handle(IIntentState<DiscreteValue> obj)
		{
			if (obj.GetValue().Intensity > 0)
			{
				IIntentState<DiscreteValue> state;
				_combinedDiscreteStates.TryGetValue(obj.GetValue().Color.ToArgb(), out state);
				if (state != null && state.Layer == _layer)
				{
					
					if (state.GetValue().Intensity < obj.GetValue().Intensity)
					{
						_combinedDiscreteStates[obj.GetValue().Color.ToArgb()] = obj;
					}
				}
				else
				{
					_combinedDiscreteStates.Add(obj.GetValue().Color.ToArgb(), obj);
				}

			}
		}

		//TODO deal with the other intent types!!!!!
	}
}
