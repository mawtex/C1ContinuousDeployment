ReportToolBarBinding.prototype = new ToolBarBinding;
ReportToolBarBinding.prototype.constructor = ReportToolBarBinding;
ReportToolBarBinding.superclass = ToolBarBinding.prototype;

ReportToolBarBinding.URL_COMPAREDIALOG = "${root}/InstalledPackages/content/views/Composite.Versioning.ContentVersioning/dialogs/dialogcompare.aspx";

/**
 * @class
 * @implements {IData}
 */
function ReportToolBarBinding () {

	/**
	 * @type {SystemLogger}
	 */
	this.logger = SystemLogger.getLogger ( "ReportToolBarBinding" );
	
	/**
	 * @type {object}
	 */
	this._selectedData = null;
	
	/*
	 * Returnable.
	 */
	return this;
}

/**
 * Identifies binding.
 */
ReportToolBarBinding.prototype.toString = function () {

	return "[ReportToolBarBinding]";
}

/**
 * @overloads {ToolBarBinding#onBindingAttach}
 */
ReportToolBarBinding.prototype.onBindingAttach = function () {

	ReportToolBarBinding.superclass.onBindingAttach.call(this);

	/*
	* File subscriptions.
	*/
	this.subscribe(ReportTableBinding.SELECTED);

	/*
	* Add action listeners.
	*/
	this.addActionListener(ButtonBinding.ACTION_COMMAND);
}

/**
 * @implements {IBroadcastListener}
 * @param {object} arg
 */
ReportToolBarBinding.prototype.handleBroadcast = function ( broadcast, arg ) {
	
	switch ( broadcast ) {
		
		case ReportTableBinding.SELECTED : 
			
			/*
			 * Cache selection. Then update toolbar 
			 * buttons and table contextmenu.
			 */
			this._selectedData = arg;
			bindingMap.broadcasterHasSelection.setDisabled ( 
				this._selectedData.viewaction == null 
			);
			break;
			
		case ReportTableBinding.COMPARE :
			
			this._doCompare ( this._selectedData, arg );
			this._compareWithOther ( false );
			break;
	}
}

/**
 * @implements {IActionListener}
 * @param {Action} action
 */
ReportToolBarBinding.prototype.handleAction = function ( action ) {
	
	var binding = action.target;
	var cover = bindingMap.cover;
	
	switch ( action.type ) {
		
		case ButtonBinding.ACTION_COMMAND :
			var command = binding.getProperty ( "cmd" );
			this.action ( command );
			break;
		
		/*
		 * Read variable on update begin...
		 */
		case UpdatePanelBinding.ACTION_UPDATING :
			this._cacheCompareMode = ReportTableBinding.isCompareMode;
			ReportTableBinding.isCompareMode = false;
			cover.show ();
			break;
		
		/*
		 * Restore variable on update finish.
		 */
		case UpdatePanelBinding.ACTION_UPDATED :
			ReportTableBinding.isCompareMode = this._cacheCompareMode;
			this._cacheCompareMode = null;
			setTimeout ( function () {
				cover.hide ();
			}, 100 );
			break;
	}2
	action.consume ();
}

/**
 * Invoke action!
 * @param {string} action
 */
ReportToolBarBinding.prototype.action = function ( action ) {
	
	switch ( action ) {
		case "view" :
			eval ( this._selectedData.viewaction );
			break;
		
		case "viewfile" :
			eval ( this._selectedData.viewfileaction );
			break;
		case "compare" :
			this._compare ();
			break;
		case "restore" :
			this._restore ();
			break;
		case "export":
			UpdateManager.isEnabled = false;
			this.bindingWindow.__doPostBack("export", "");
			UpdateManager.isEnabled = true;
			break;
	}
}

/**
 * Compare versions.
 */
ReportToolBarBinding.prototype._compare = function () {
	
	var data = bindingMap.versions.getCurrentData ();
	var isLatestVersion = data.index == this._selectedData.index;
	
	var self = this;
	Dialog.invokeModal ( ReportToolBarBinding.URL_COMPAREDIALOG, {
		handleDialogResponse : function ( response, result ) {
			if ( response == Dialog.RESPONSE_ACCEPT ) {
				switch ( result.get ( "comparison" )) {
					case "current" :
						self._compareWithCurrent ();
						break;
					case "other" :
						self._compareWithOther ( true );
						break;
				}
			}
		}
	}, isLatestVersion);
}

/**
 * Compare with current.
 */
ReportToolBarBinding.prototype._compareWithCurrent = function () {
	
	var data = bindingMap.versions.getCurrentData ();
	this._doCompare ( this._selectedData, data );
}

/**
 * Compare with other. This delegates responsibility to the table binding...
 * @param {boolean} isCompare
 */
ReportToolBarBinding.prototype._compareWithOther = function ( isCompare ) {
	
	if ( isCompare ) {
		this.subscribe ( ReportTableBinding.COMPARE );
		bindingMap.versions.compare ( true );
	} else {
		this.unsubscribe ( ReportTableBinding.COMPARE );
		bindingMap.versions.compare ( false );
	}
}

/**
 * Restore version.
 */
ReportToolBarBinding.prototype._restore = function () {
	
	var isRestored = false;
	
	var self = this;
	Dialog.question ( 
		StringBundle.getString ( "Composite.Versioning.ContentVersioning", "RestoreDialog.Title" ), 
		StringBundle.getString ( "Composite.Versioning.ContentVersioning", "RestoreDialog.Text" ), 
		Dialog.BUTTONS_ACCEPT_CANCEL, 
		{
			handleDialogResponse : function ( response ) {
				if ( response == Dialog.RESPONSE_ACCEPT ) {
					if ( !isRestored ) {
						isRestored = true;
						eval ( self._selectedData.restoreaction );
					}
				}
			}
		}
	);
}

/**
 * Execute comparison. 
 * @param {object} data1
 * @param {object} data2
 */
ReportToolBarBinding.prototype._doCompare = function ( data1, data2 ) {
	
	/*
	 * It appears that the backend is argument-ordering-insensitive. 
	 * This trick seems to get the ordering right (meaning that 
	 * first and last selected row should be interchangable for 
	 * the same result).
	 */
	var d1 = data1.index < data2.index ? data1 : data2;
	var d2 = data1.index < data2.index ? data2 : data1;

	VersioningReport.Show("", d1.comparetoken, d2.comparetotoken, d1.piggybag, d1.piggybagHash);
}