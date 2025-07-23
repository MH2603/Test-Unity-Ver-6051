mergeInto(LibraryManager.library, {

	StartWebGLhTracker: function(name)
	{
        if(!window.hTracker){
            console.error('%chTracker not found! Please make sure to use the hTracker WebGLTemplate in your ProjectSettings','font-size: 32px; font-weight: bold');
            throw new Error("Tracker not found! Please make sure to use the hTracker WebGLTemplate in your ProjectSettings");
            return;
        }

    	window.hTracker.startTracker(UTF8ToString(name));
    },
    StopWebGLhTracker: function()
	{
    	window.hTracker.stopTracker();
    },
    IsWebGLhTrackerReady: function()
    {
        return window.hTracker != null;
    },
    SetWebGLhTrackerSettings: function(settings)
	{
    	window.hTracker.setTrackerSettings(UTF8ToString(settings), "1.0.2.952084");
    },
});
