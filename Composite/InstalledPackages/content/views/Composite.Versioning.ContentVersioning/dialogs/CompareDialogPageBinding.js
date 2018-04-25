CompareDialogPageBinding.prototype = new DialogPageBinding;
CompareDialogPageBinding.prototype.constructor = CompareDialogPageBinding;
CompareDialogPageBinding.superclass = DialogPageBinding.prototype;

/**
 * @class
 * @implements {IData}
 */
function CompareDialogPageBinding () {

	/**
	 * @type {SystemLogger}
	 */
	this.logger = SystemLogger.getLogger ( "CompareDialogPageBinding" );
	
	/*
	 * Returnable.
	 */
	return this;
}

/**
 * Identifies binding.
 */
CompareDialogPageBinding.prototype.toString = function () {

	return "[CompareDialogPageBinding]";
}

/** 
 * If latest version is selected, the "compare with latest" option will be disabled.
 * @overloads {DialogPageBinding#setPageArgument}
 * @param {object} arg
 */
CompareDialogPageBinding.prototype.setPageArgument = function ( arg ) {
	
	CompareDialogPageBinding.superclass.setPageArgument.call ( this, arg );
	
	if ( arg == true ) {
		bindingMap.currentoption.disable ();
		bindingMap.otheroption.check ();
	} else {
		bindingMap.currentoption.check ();
	}
}