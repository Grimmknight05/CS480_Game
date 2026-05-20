// ISceneCommand.cs
using System.Collections;

public interface ISceneCommand
{
    IEnumerator Execute(LevelManager manager);
}