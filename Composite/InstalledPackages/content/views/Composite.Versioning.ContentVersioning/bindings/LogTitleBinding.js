LogTitleBinding.prototype = new LabelBinding;
LogTitleBinding.prototype.constructor = LogTitleBinding;
LogTitleBinding.superclass = LabelBinding.prototype;

/**
* @class
* @implements {IData}
*/
function LogTitleBinding() {

	/**
	* @type {SystemLogger}
	*/
	this.logger = SystemLogger.getLogger("LogTitleBinding");

	/**.
	* @type {string}
	*/
	this.entityToken = null;

	/*
	* Returnable.
	*/
	return this;
}

/**
* Identifies binding.
*/
LogTitleBinding.prototype.toString = function () {

	return "[LogTitleBinding]";
};

/**
* Overloads {Binding#onBindingAttach}
*/
LogTitleBinding.prototype.onBindingAttach = function () {

	LogTitleBinding.superclass.onBindingAttach.call(this);

	this.entitytoken = this.getProperty("entitytoken");
	
	if (this.entitytoken) {
		this.addEventListener(DOMEvents.CLICK);

		this.attachClassName("entitytitle");
	}
};


/** 
* @overloads {LabelBinding#onBindingRegister}
*/
LogTitleBinding.prototype.onBindingRegister = function () {

	LogTitleBinding.superclass.onBindingRegister.call(this);

	
};

/**
* @implements {IEventListener}
* @overloads {Binding#handleEvent}
* @param {MouseEvent} e
*/
LogTitleBinding.prototype.handleEvent = function (e) {

	LogTitleBinding.superclass.handleEvent.call(this, e);

	switch (e.type) {
		case DOMEvents.CLICK:
			if (this.entitytoken) {
				
				var entitytoken = this.entitytoken;

				if (ExplorerBinding.bindingInstance.getPerspectives) {
					var perspectives = ExplorerBinding.bindingInstance.getPerspectives();
					var targetHandle = null;

					perspectives.each(function (handle, button) {
						var rootToken = button.node.getEntityToken();
						var arg = [];
						arg.push({
							ProviderName: button.node.getProviderName(),
							EntityToken: button.node.getEntityToken(),
							Piggybag: button.node.getPiggyBag()
						});

						var response = TreeService.FindEntityToken(rootToken, entitytoken, arg);
						if (response.length) {
							targetHandle = handle;
							return false;
						};
						return true;
					});
					if (targetHandle) {
						ExplorerBinding.bindingInstance.setSelectionByHandle(targetHandle);
						setTimeout(function () {
							EventBroadcaster.broadcast(
								BroadcastMessages.SYSTEMTREEBINDING_FOCUS,
								entitytoken
							);
						}, 125);
					}

				} else {
					//obsolute, for supporting C1 < 5.0
					var targetPerspectiveButton = null;
					var perspectiveButtons = new List();
					ExplorerBinding.bindingInstance.getDescendantBindingsByType(ExplorerToolBarButtonBinding)
						.each(function(explorerbutton) {
							if (explorerbutton.isVisible) {
								perspectiveButtons.add(explorerbutton);
							}
						});
				

					perspectiveButtons.each(function(perspectiveButton) {
						var rootToken = perspectiveButton.node.getEntityToken();
						var arg = [];
						arg.push({
							ProviderName: perspectiveButton.node.getProviderName(),
							EntityToken: perspectiveButton.node.getEntityToken(),
							Piggybag: perspectiveButton.node.getPiggyBag()
						});

						var response = TreeService.FindEntityToken(rootToken, entitytoken, arg);
						if (response.length) {
							targetPerspectiveButton = perspectiveButton;
							return false;
						};
						return true;
					});
					if (targetPerspectiveButton) {
						targetPerspectiveButton.fireCommand();
						setTimeout(function() {
							EventBroadcaster.broadcast(
								BroadcastMessages.SYSTEMTREEBINDING_FOCUS,
								entitytoken
							);
						}, 125);
					}
				}
			}
			;
			break;
	}
}