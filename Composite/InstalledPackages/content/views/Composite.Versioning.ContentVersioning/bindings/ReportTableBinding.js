ReportTableBinding.prototype = new Binding;
ReportTableBinding.prototype.constructor = ReportTableBinding;
ReportTableBinding.superclass = Binding.prototype;

ReportTableBinding.SELECTED = "reporttable version selected";
ReportTableBinding.COMPARE = "reporttable compare selected";

ReportTableBinding.CLASSNAME_COMPAREMODE = "comparemode";

ReportTableBinding.selectedIndex = null;
ReportTableBinding.scrollPoint = null;

/**
 * True when version comparison is active.
 * @type {boolean}
 */
ReportTableBinding.isCompareMode = false;

/**
 * Because MS AJAX will reload bindings desperately,   
 * this method must reside on a non-destructable object. 
 * Considered private to this class.
 */
ReportTableBinding._eventHandler = {
	
	/*
	 * @implements {IEventHandler}
	 * @param {MouseEvent} e
	 */
	handleEvent : function ( e ) {
	 
		var target = e.currentTarget ? e.currentTarget : DOMEvents.getTarget ( e );
		var scrollbox = window.bindingMap.scrollbox.bindingElement
		var cursor = window.bindingMap.comparecursor;	
		
		if ( ReportTableBinding.isCompareMode ) {
		
			switch ( e.type ) {
				
				case DOMEvents.MOUSEMOVE :
					cursor.setPosition ( 
						DOMUtil.getGlobalMousePosition ( e )
					);
					break;
				
				case DOMEvents.MOUSEENTER :
				case DOMEvents.MOUSEOVER :
					if ( target == scrollbox ) {
						cursor.fadeIn ();
					}
					break;
					
				case DOMEvents.MOUSELEAVE :
				case DOMEvents.MOUSEOUT :
					if ( target == scrollbox ) {
						cursor.fadeOut ();
					}
					break;
			}
		}
	}
}

/**
 * @class
 * @implements {IData}
 */
function ReportTableBinding () {

	/**
	 * @type {SystemLogger}
	 */
	this.logger = SystemLogger.getLogger ( "ReportTableBinding" );
	
	/**
	 * The selected row.
	 * @type {HTMLTableRowElement}
	 */
	this._selection = null;
	
	/**
	 * The row to compare with.
	 * @type {HTMLTableRowElement}
	 */
	this._comparison = null;
	
	/**
	 * @overwrites {Binding#isActivationAware}
	 * @implements {IActivationAware}
	 */
	this.isActivationAware = true;
	
	/**
	 * @overwrites {Binding#isActivated}
	 * @implements {IActivationAware}
	 */
	this.isActivated = false;
	
	/*
	 * Returnable.
	 */
	return this;
}

/**
 * Identifies binding.
 */
ReportTableBinding.prototype.toString = function () {

	return "[ReportTableBinding]";
}

/**
 * @overloads {Binding#onBindingAttach}
 */
ReportTableBinding.prototype.onBindingAttach = function () {
	
	ReportTableBinding.superclass.onBindingAttach.call ( this );
	
	var row, rows = new List ( this.bindingElement.rows );
	
	/*
	 * Restore scroll position?
	 */
	var point = ReportTableBinding.scrollPoint; 
	if ( point != null ) {
		bindingMap.scrollbox.setPosition ( point );
	}
	
	/*
	 * Restore selected row?
	 */
	var index = ReportTableBinding.selectedIndex;
	if ( index != null ){
		row = rows.get ( index );
		if ( row != null ) {
			this._select ( row );
		}
	}
	
	/*
	 * Inject DOM events and index tokens. The latter 
	 * is used to determine newest versus oldest entry. 
	 * @see {ReportTableBinding#isTokenNewer}
	 */
	while ( rows.hasNext ()) {
		row = rows.getNext ();
		DOMEvents.addEventListener ( row, DOMEvents.MOUSEENTER, this );
		DOMEvents.addEventListener ( row, DOMEvents.MOUSELEAVE, this );
		DOMEvents.addEventListener ( row, DOMEvents.MOUSEDOWN, this );
	}
}

/**
 * Store the scroll-position when table reloads in usual dramatic MS AJAX style.
 * @overloads {Binding#onBindingDispose}
 */
ReportTableBinding.prototype.onBindingDispose = function () {
	
	ReportTableBinding.superclass.onBindingDispose.call ( this );
	ReportTableBinding.scrollPoint = bindingMap.scrollbox.getPosition ();
	if ( ReportTableBinding.isCompareMode ) {
		this.compare ( false );
	}
}


/**
 * Enable keyboard navigation. 
 * @implements {IActivationAware}
 */
ReportTableBinding.prototype.onActivate = function () {
	
	if ( !this.isActivated ) {
		this.subscribe ( BroadcastMessages.KEY_ARROW );
		this.subscribe ( BroadcastMessages.KEY_ENTER );
		this.detachClassName ( "deactivated" );
		this.isActivated = true;
	}
}

/**
 * Disable keyboard navigation.
 * @implements {IActivationAware} 
 */
ReportTableBinding.prototype.onDeactivate = function () {
	
	if ( this.isActivated ) {
		this.unsubscribe ( BroadcastMessages.KEY_ARROW );
		this.unsubscribe ( BroadcastMessages.KEY_ENTER );
		this.attachClassName ( "deactivated" );
		this.isActivated = false;
	}
}

/**
 * @implements {IEventHandler}
 * @param {MouseEvent} e
 */
ReportTableBinding.prototype.handleEvent = function ( e ) {
	
	var target = e.currentTarget ? e.currentTarget : DOMEvents.getTarget ( e );
	
	switch ( e.type ) {
		
		case DOMEvents.MOUSEENTER :
		case DOMEvents.MOUSEOVER :
			if ( target != this._selection && target != this._comparison ) {
				target.className = "hilite";
			}
			break;
			
		case DOMEvents.MOUSELEAVE :
		case DOMEvents.MOUSEOUT :
			if ( target != this._selection && target != this._comparison ) {
				target.className = "";
			}
			break;
			
		case DOMEvents.MOUSEDOWN :
			if ( this._comparison != null ) {
				this._comparison.className = "";
			}
			var row = target;
			if ( Client.isExplorer ) {
				while ( row.nodeName != "TR" ) {
					row = row.parentNode;
				}
			}
			if ( row != this._selection ) {
				this._select ( row );
			}
			break;
	}
}

/**
 * 
 * @implements {IBroadcastListener}
 * @param {string} broadcast
 * @param {object} arg
 * @return
 */
ReportTableBinding.prototype.handleBroadcast = function ( broadcast, arg ) {
	
	if ( bindingMap.root.isActivated && this._selection != null ) {
		if ( UserInterface.isBindingVisible ( this )) {
			switch ( broadcast ) {
				case BroadcastMessages.KEY_ARROW :
					switch ( arg ) {
						case KeyEventCodes.VK_UP :
						case KeyEventCodes.VK_DOWN :
							this._navigate ( arg == KeyEventCodes.VK_UP );
							break;
					}
					break;
				case BroadcastMessages.KEY_ENTER :
					bindingMap.toolbar.action ( "view" );
					break;
			}
		}
	}
}

/**
 * Keyboar navigation.
 * @param {boolean} isUp
 */
ReportTableBinding.prototype._navigate = function ( isUp ) {

	var index = this._selection.rowIndex + ( isUp ? -1 : 1 );
	if ( index > -1 ) {
		var row = this.bindingElement.rows [ index ];
		if ( row != null ) {
			this._select ( row );
			row.scrollIntoView ( isUp ); // TODO: check something first!
		}
	}
}

/**
 * Start or stop compare-mode. Invoked by the ReportToolBarBinding.
 * @type {boolean}
 */
ReportTableBinding.prototype.compare = function ( isCompare ) {

	ReportTableBinding.isCompareMode = isCompare;
	
	var action = isCompare ? "addEventListener" : "removeEventListener";
	var scrollbox = bindingMap.scrollbox.getBindingElement ();
	
	var handler = ReportTableBinding._eventHandler;
	DOMEvents [ action ] ( scrollbox, DOMEvents.MOUSEMOVE, handler );
	DOMEvents [ action ] ( scrollbox, DOMEvents.MOUSEENTER, handler );
	DOMEvents [ action ] ( scrollbox, DOMEvents.MOUSELEAVE, handler );
	
	if ( isCompare ) {
		bindingMap.comparecursor.show ();
	} else {
		bindingMap.comparecursor.hide ();
		if ( this._comparison != null ) {
			this._comparison.className = "";
		}
	}
	
	/*
	 * Hopefully, this will fix the 
	 * glitch where cursor is gone.
	 */
	var win = this.bindingWindow;
	setTimeout ( function () {
		win.focus ();
	}, 1000 );
}

/**
 * @param {HTMLTableRowElement} element
 */
ReportTableBinding.prototype._select = function ( row ) {
	
	/*
	 * Cleanup previous selection.
	 */
	if ( !ReportTableBinding.isCompareMode ) {
		if ( this._selection != null ) {
			this._selection.className = "";
		}
		this._selection = row;
		ReportTableBinding.selectedIndex = row.rowIndex;
	} else{
		this._comparison = row;
	}
	
	/*
	 * Highlight row.
	 */
	row.className = "selected";
	
	/*
	 * Broadcast for ReportToolBarBinding to intercept...
	 */
	if ( ReportTableBinding.isCompareMode ) {
		
		var comparetotoken = row.getAttribute ( "comparetotoken" );
		if ( comparetotoken != null ) {
			
			EventBroadcaster.broadcast ( ReportTableBinding.COMPARE, this._getData ( row ));
			this._comparison = null;
			
		} else {
			
			Dialog.warning ( 
				StringBundle.getString ( "Composite.Versioning.ContentVersioning", "CompareErrorDialog.Title" ), 
				StringBundle.getString ( "Composite.Versioning.ContentVersioning", "CompareErrorDialog.Text" ),
				Dialog.BUTTONS_ACCEPT,
				{
					handleDialogResponse : function () {
						row.className = "";
					}
				}
			);
		}
		
	} else {
		
		/*
		 * Update toolbar and contextmenu.
		 */
		EventBroadcaster.broadcast ( ReportTableBinding.SELECTED, this._getData ( row ));
	}
}

/** 
 * Get data for row.
 * @param {HTMLTableRowElement} row
 * @return {object}
 */
ReportTableBinding.prototype._getData = function ( row ) {
	
	var result = null;
	
	if ( row != null ) {
		result = {
			index			: row.rowIndex,
			viewaction 		: row.getAttribute ( "viewaction" ),
			viewfileaction 		: row.getAttribute ( "viewfileaction" ),
			restoreaction 	: row.getAttribute ( "restoreaction" ),
			compareaction 	: row.getAttribute ( "compareaction" ),
			comparetoken	: row.getAttribute ( "comparetoken" ),
			comparetotoken	: row.getAttribute ( "comparetotoken" ),
			piggybag : row.getAttribute("piggybag"),
			piggybagHash : row.getAttribute("piggybagHash")
		};
	}
	return result;
}

/**
 * Get data for newest version.
 * @return {object}
 */
ReportTableBinding.prototype.getCurrentData = function () {
	
	var result = null;
	var i = 0;
	
	/*
	 * Traverse table until we find a usable row.
	 */
	while ( result == null ) {
		var row = this.bindingElement.rows [ i++ ];
		var data = this._getData ( row );
		if ( data.viewaction != null ) {
			result = data;
		}
	}
	return result;
}


/**
 * Always update selector!
 * TODO: Not a good idea, since this control is pretty render-heavy...
 * @overwrites {Binding#handleElement}
 * @implements {IUpdateHandler}
 * @param {Element} element
 * @returns {boolean} Return true to trigger method handleElement.
 */
ReportTableBinding.prototype.handleElement = function () {

	return true;
}

/**
 * Always update table!
 * @implements {IUpdateHandler}
 * @overwrites {Binding#updateElement}
 * @param {Element} element
 * @param {Element} oldelement
 * @returns {boolean} Return true to stop crawling.
 */
ReportTableBinding.prototype.updateElement = function (element, oldelement) {
	//fix bug - button not unregister on detach
	
	var buttons = this.getDescendantBindingsByType(ButtonBinding);
	buttons.each(function (button) {
		var callbackid = button.getProperty("callbackid");
		if (callbackid != null) {
			button.bindingWindow.DataManager.unRegisterDataBinding(callbackid, button);
		}

	});


	this.detachRecursive();
	this.bindingElement.innerHTML = "";

	/*
	 * ALWAYS update selector (full replace)
	 */
	this.bindingWindow.UpdateManager.addUpdate(
			new this.bindingWindow.ReplaceUpdate(this.getID(), element)
	);
	return true;
}