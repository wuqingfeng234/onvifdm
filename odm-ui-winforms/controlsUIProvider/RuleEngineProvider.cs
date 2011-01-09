﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using odm.models;
using odm.controls;
using System.Windows.Forms;
using odm.controllers;

namespace odm.controls.UIProvider {
	public class RuleEngineProvider : BaseUIProvider{
		PropertyRuleEngine _ruleEngine;
		public void InitView(RuleEngineModel devModel, DataProcessInfo datProcInfo, Action ApplyChanges, Action CancelChanges) {
			_ruleEngine = new PropertyRuleEngine(devModel) { Dock = DockStyle.Fill,  Save = ApplyChanges,
															 Cancel = CancelChanges,
															 onBindingError = BindingError
			};
			if (datProcInfo != null)
				_ruleEngine.memFile = datProcInfo.VideoProcessFile;
			UIProvider.Instance.MainFrameProvider.AddPropertyControl(_ruleEngine);
		}
		public override void ReleaseUI() {
			if (_ruleEngine != null && !_ruleEngine.IsDisposed)
				_ruleEngine.ReleaseAll();
		}
	}
}
