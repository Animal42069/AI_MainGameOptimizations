# AI_MainGameOptimizations
This plugin addresses framerate problems with AIS, in an attempt to make the main game more playable.  The goal is to improve framerate without making too many sacrifices in graphical quality.  The changes are pretty extensive, so there may be bugs lurking in the code... check back often for updates.  All changes can be customized if you don't like the behavior.  Using the default settings I've provided, I go from about 10-20 fps on the main island to about 30-40 fps.  There are still occasional hang ups, mainly when the characters are being loaded/changing clothes.  Below is a list of all of the optimizations this plugin does:

# Character Optimizations
Disable all dynamic bones on all characters by visibility and range.  Illusion was disabling certain dynamic bones by visibility, but user added dynamic bones were always enabled.  Characters in HScenes have all dynamic bones enabled.<br>
Cull animations of non-visibile characters.<br>
Skip IK solvers for non-visible characters.<br>
Skip head direction solvers for non-visible characters.<br>
Throttle AI checks to see what items are nearby that can be interacted with.<br>

# World Optimizations
Move the main island terrain component to a new layer where it can be clipped.  The terrain component is just the mesh that you walk on and contains most of the trees.  It is not the same component as the mountaints/large terrain features.  It can be clipped rather aggessively without noticable effects.<br>
Adjust other terrain features like level of detail, tree visibility, etc.  Default settings are the same as Illussion's, but they can be tweaked down to increase framerate more.<br>
Disable main island city point lights.  Main island lights are composed of a spot light and a point light that cast shadows.  Point lights that cast shadows have a huge CPU cost.  The intensity of the spotlight component is increased to make up for the loss of light.<br>
Cull world animations that are not visible.<br>
Cull animal animations that are not visible.<br>
Disable particles that are not visible and far away.<br>

# Housing Optimizations
Housing items are very poorly optimized and a lot of them can kill your fps pretty quickly.  Several optimizations are performed to try to mitigate their effects
Housing items can be placed on 3 new layers, small, medium, and large objects.  These layers can be individually configured for clip distance and shadow distance.  This is important because normally housing items are placed on the main map layer, meaning they are rendered at all times.<br>
Camera colliders and sound effect colliders for housing items are disabled when the player moves far enough away from them, so they aren't needlessly checked for collision every frame.<br>
Cull housing animations that are not visible.<br>
Disable housing particles that are not visible and far away.<br>

# Camera and Shadow Optimizations
These optimizations work along with the new layers that world and housing optimizations utilize.<br>
All important game layers can have their clip distance and shadow distance individually customized.  The stock default settings included work well for the main island without too much noticable effects of items popping into visibility.  The character layer is perhaps the most important to adjust, because they are the most expensive to render and update.  This layer can be adjusted lower to increase performance furter, at the cost of characters popping on the screen occasionally.<br>
Option to use spherical clipping, which is enabled by default as the cost of using it far outweights the penaly of having items pop into view when the camera spins.<br>
HScene camera clipping.  Option to have a global clipping value that only gets applied during HScenes.  This lets you set up a pretty aggressive camera clip during HScenes for improved performane... you're not staring off at the trees in the distance right now, are you?<br>

# Player/UI Optimizations
Disables UI Windows that are not being used/not visible.  Default Illusion behavior is to set alpha to 0 when UI is not being used.  This means that they are still being rendered, they just aren't being displayed.<br>
Disables Minimap rendering when it is not visible.  I highly recommend not using the minimap.  Even with these optimizations, it takes 2-3ms to render the minimap.<br>
Disable player dynamic bones.<br>
Throttle checks to see if a nearby item can be searched/interacted with.<br>

