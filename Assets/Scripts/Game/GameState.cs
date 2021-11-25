using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameState
{
    public State CurrentState { get; set; } = State.Start;
    public Scene CurrentScene { get; set; } = Scene.MainMenu;

    public enum State
    {
        Start,
        OnPlay,
        Paused,
    }

    public enum Scene
    {
        MainMenu = 0,
        Game = 1,
    }
}
