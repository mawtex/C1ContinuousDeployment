ReportLabelBinding.prototype = new LabelBinding;
ReportLabelBinding.prototype.constructor = ReportLabelBinding;
ReportLabelBinding.superclass = LabelBinding.prototype;

/*
 * Notice this hardcoded global variable! 
 * TODO: Move it to some kind of class object.
 * TODO: Make it dynamic (pages or datatypes). 
 */
window.versioningscenario = "pages";

/*
 * Icon library.
 */
ReportLabelBinding.IMAGES = {
	
	/*
	 * Icons for pages
	 */
	pages : {
		task : {
			Add 				: "page-add-page",
			Edit 				: "page-edit-page",
			SendToDraft			: "item-send-back-to-draft",
			SendForApproval 	: "item-send-forward-for-approval",
			SendForPublication 	: "item-send-forward-for-publication",
			Publish 			: "item-publish",
			Unpublish 			: "item-unpublish",
			Rollback			: "versioning-restore",
			Delete				: "page-delete-page"
		},
		activity : {
			Add					: "page-add-page",
			Save				: "save"
		}
	}
}

/**
 * @class
 * @implements {IData}
 */
function ReportLabelBinding () {

	/**
	 * @type {SystemLogger}
	 */
	this.logger = SystemLogger.getLogger ( "ReportLabelBinding" );
	
	/*
	 * Returnable.
	 */
	return this;
}

/**
 * Identifies binding.
 */
ReportLabelBinding.prototype.toString = function () {

	return "[ReportLabelBinding]";
}

/** 
 * @overloads {LabelBinding#onBindingRegister}
 */
ReportLabelBinding.prototype.onBindingRegister = function () {
	
	ReportLabelBinding.superclass.onBindingRegister.call ( this );
	
    var role = this.getProperty("versioningrole");
	var imageKey = this.getProperty("imagekey");
    var image = this._getReportImage(role, imageKey );
	
	/*
	 * Compute image.
	 */
	if ( image != null ) {
		image = "${icon:" + image + "}"; 
	} else {
		image = "${icon:default}";
	}
	this.setProperty ( "image", image );
	
	/*
	 * Bold label for "Publish" task.
	 */
    if ( imageKey == "Publish" ) {
		this.bindingElement.style.fontWeight = "bold";
	}
}

/**
 * Compute image.
 */
ReportLabelBinding.prototype._getReportImage = function (role, imageKey) {
	
	var result = null;
	var bank = ReportLabelBinding.IMAGES [ window.versioningscenario ];
	var group = bank [ role ];
	if ( group != null ) {
        result = group[imageKey];
	}
	return result;
}