# λ-μ 
## Half Micro FPS

This branch adds features to the Unity Micro FPS to make it more of a complete game.

## DunGenMap

**DunGenMap** is a map generator designed for Unity games. 

### Features

- **Simple Text File**: Use [moebius], [rexpaint], or [pablodraw] to draw the maps. You can even use your favorite text editor (of course it's vi).
- **Characters Create Objects**: Text characters get mapped to game objects by calling them `chr-#`, `chr-d`, etc.
- **Reference Game Object**: An empty game object can be created using `DunGenRef` to reference another prefab for complex game objects.
- **Reload Map**: The map can be reloaded in Unity editor as it changes.

[moebius]: https://github.com/blocktronics/moebius
[rexpaint]: https://kyzrati.itch.io/rexpaint
[pablodraw]: https://picoe.ca/products/pablodraw

### TwineStory

See [Cradle/TwineStory] for a feature-rich library. This `TwineStory` uses the Twine Twee export to parse basic dialog and stories, for dialog or text interaction in a game, without any third-party dependencies.

[Cradle/TwineStory]: https://github.com/daterre/Cradle