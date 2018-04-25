LogTableBinding.prototype = new ReportTableBinding;
LogTableBinding.prototype.constructor = LogTableBinding;
LogTableBinding.superclass = ReportTableBinding.prototype;

/**
* @class
* @implements {IData}
*/
function LogTableBinding() {

	/**
	* @type {SystemLogger}
	*/
	this.logger = SystemLogger.getLogger("LogTableBinding");

	/*
	* Returnable.
	*/
	return this;
}

/**
* Identifies binding.
*/
LogTableBinding.prototype.toString = function() {

	return "[LogTableBinding]";
};

/** 
 * Get data for row.
 * @param {HTMLTableRowElement} row
 * @return {object}
 */
ReportTableBinding.prototype._getData = function (row) {

	var result = null;

	if (row != null) {
		result = {
			index: row.rowIndex,
			restoreaction: row.getAttribute("restoreaction")
		};
	}
	return result;
}