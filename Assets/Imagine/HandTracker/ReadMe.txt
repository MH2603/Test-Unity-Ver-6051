Follow the steps below to setup your hand tracking scene:

1.  Create a new scene, and delete your MainCamera
2.  Drag an ARCamera prefab to your scene (from Assets/Imagine/Common/Prefabs)
3.  Drag a HandTracker prefab to your scene (from Assets/Imagine/HandTracker/Prefabs). 
    Then drag the ARCamera to the TrackerCam property of the hand tracker
4.  Position and parent your gameobjects in their corresponding hand/finger joints 
    eg. Wrist Watch - Joint 0, Ring - Joint 13

5.  You can enable masking (Experimental) by changing your handmesh material shader to Imagine/Depth Mask
6.  Go to ProjectSettings and change your WebGLTemplate to HandTracker
7.  Buildand-Run your project

For any technical inquiries, contact our team in Discord: https://discord.com/invite/ypNARJJEbB

A more detailed pdf guide will be released soon.
