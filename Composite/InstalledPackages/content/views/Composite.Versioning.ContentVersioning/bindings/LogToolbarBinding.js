LogToolBarBinding.prototype = new ReportToolBarBinding;
LogToolBarBinding.prototype.constructor = LogToolBarBinding;
LogToolBarBinding.superclass = ReportToolBarBinding.prototype;

/**
 * @class
 * @implements {IData}
 */
function LogToolBarBinding() {

	/**
	 * @type {SystemLogger}
	 */
	this.logger = SystemLogger.getLogger("LogToolBarBinding");

	/*
	 * Returnable.
	 */
	return this;
}

/**
 * Identifies binding.
 */
LogToolBarBinding.prototype.toString = function () {

	return "[LogToolBarBinding]";
}


/**
 * @overloads {IBroadcastListener}
 * @param {object} arg
 */
ReportToolBarBinding.prototype.handleBroadcast = function (broadcast, arg) {

	switch (broadcast) {
		case ReportTableBinding.SELECTED:

			/*
			 * Cache selection. Then update toolbar
			 * buttons and table contextmenu.
			 */
			this._selectedData = arg;
			bindingMap.broadcasterHasSelection.setDisabled(
					this._selectedData.restoreaction == null
			);
			break;
	}
}

/**
 * overloads
 * @param {string} action
 */
LogToolBarBinding.prototype.action = function (action) {

	switch (action) {
		case "restore":
			this._restore();
			break;
		case "export":
			UpdateManager.isEnabled = false;
			this.bindingWindow.__doPostBack("export", "");
			UpdateManager.isEnabled = true;

			break;
	}
}



/**
 * Restore version.
 */
LogToolBarBinding.prototype._restore = function () {

	var isRestored = false;

	var self = this;
	Dialog.question(
		StringBundle.getString("Composite.Versioning.ContentVersioning", "RestoreDeletedDialog.Title"),
		StringBundle.getString("Composite.Versioning.ContentVersioning", "RestoreDeletedDialog.Text"),
		Dialog.BUTTONS_ACCEPT_CANCEL,
		{
			handleDialogResponse: function (response) {
				if (response == Dialog.RESPONSE_ACCEPT) {
					if (!isRestored) {
						isRestored = true;
						eval(self._selectedData.restoreaction);
					}
				}
			}
		}
	);
}