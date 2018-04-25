ComparePageBinding.prototype = new PageBinding;
ComparePageBinding.prototype.constructor = ComparePageBinding;
ComparePageBinding.superclass = PageBinding.prototype;

ComparePageBinding.URL_COMPARE = "${root}/content/dialogs/util/comparestrings/comparestrings.aspx";

/**
 * @class
 * @implements {IData}
 */
function ComparePageBinding () {

	/**
	 * @type {SystemLogger}
	 */
	this.logger = SystemLogger.getLogger("ComparePageBinding");

	/**
	 * @type {Boolean}
	 */
	this.isPrintReady = false;
	
	/*
	 * Returnable.
	 */
	return this;
}

/**
 * Identifies binding.
 */
ComparePageBinding.prototype.toString = function () {

	return "[ComparePageBinding]";
}

ComparePageBinding.prototype.print = function () {

	if (!this.isPrintReady) {
		if (top.DiffService == null) {
			top.DiffService = WebServiceProxy.createProxy(Constants.URL_WSDL_DIFFSERVICE);
		}
		var self = this;
		var links = this.bindingElement.querySelectorAll("a.compare");
		new List(links).each(function (link) {
			var oldval = link.getAttribute("oldval");
			var newval = link.getAttribute("newval");
			markup = self._getMarkup(
				new List(
					top.DiffService.GetDifference(oldval, newval, "\n")
				)
			);
			var container = self.bindingDocument.createElement("div");
			container.className = "print";
			container.innerHTML = markup;
			link.parentNode.appendChild(container);
		})
		this.isPrintReady = true;
	}
	this.bindingWindow.print();
}


/**
 * Build markup for a real diff-comparison.
 * @param {List<object>} diffs
 */
ComparePageBinding.prototype._getMarkup = function (diffs) {

	var markup = "";
	//var markup = "<pre>";
	var linecount = 0;
	var self = this;

	diffs.each(function (diff) {

		markup += "<span class=\"type" + String(diff.DiffType) + "\" ";
		switch (diff.DiffType) {
			case 1:
				markup += "title=\"This text was DELETED between two versions\" ";
				break;
			case 2:
				markup += "title=\"This text was ADDED between two versions\" ";
				break;
		}
		markup += ">";
		var lines = new List(diff.Lines);
		lines.each(function (line) {
			if (line != null) {
				markup += self._encode(line) + "<br/>";
			}
		});
		markup += "</span>";
	});

	//markup += "</pre>";
	return markup;
}

/**
 * HTML-encode line. And some other tricks.
 * @param {string} line
 * @return {string}
 */
ComparePageBinding.prototype._encode = function (line) {

	line = line.replace(/&/g, "&amp;");
	line = line.replace(/\"/g, "&quot;");
	line = line.replace(/</g, "&lt;");
	line = line.replace(/>/g, "&gt;");

	/*
	 * Note that we boldly assume all TABS to be
	 * locating in the leading section of the line.
	 */
	var index = line.lastIndexOf("\t");
	if (index > -1) {
		line = line.substring(0, index) + "<em>" + line.substring(index + 1, line.length) + "</em>";
		line = line.replace(/\t/g, "&nbsp;&nbsp;&nbsp;&nbsp;");
	} else {
		line = "<em>" + line + "</em>";
	}

	return line;
}

/**
 * Compare strings.
 * @param {HTMLAnchorElement} link
 */
ComparePageBinding.prototype.compare = function ( link ) {
	
	var oldval = link.getAttribute ( "oldval" );
	var newval = link.getAttribute ( "newval" );

	Dialog.invokeModal ( ComparePageBinding.URL_COMPARE, null, {
		string1 : oldval,
		string2 : newval
	});
}