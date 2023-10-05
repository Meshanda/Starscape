
public class GameManager : Singleton<GameManager>
{
	public DatabaseSO database;
	public Player player;

    public void Start()
    {
        SoundManager.OnStartMusic();
    }
}