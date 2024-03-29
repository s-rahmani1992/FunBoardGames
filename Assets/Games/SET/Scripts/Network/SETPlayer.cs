using FishNet.Object;
using FishNet.Object.Synchronizing;
using System;

// NOTE: Do not put objects in DontDestroyOnLoad (DDOL) in Awake.  You can do that in Start instead.
namespace FunBoardGames.SET
{
    public class SETPlayer : BoardGamePlayer
    {
        public event Action<int, int> WrongGuessChanged;
        public event Action<int, int> CorrectGuessChanged;
        public event Action<bool> GuessChanged;
        public event Action<VoteAnswer, VoteAnswer> VoteChanged;

        public int Score => CorrectCount - WrongCount;

        #region Syncvars

        [field: SyncVar( OnChange = nameof(PlayerWrongChanged))]
        public byte WrongCount { get; private set; }

        [field: SyncVar(OnChange = nameof(PlayerCorrectChanged))]
        public byte CorrectCount { get; private set; }

        [field: SyncVar(OnChange = nameof(OnGuessChanged))]
        public bool IsGuessing { get; private set; }

        [field: SyncVar(OnChange = nameof(OnVoteChanged))]
        public VoteAnswer VoteAnswer { get; private set; }

        void PlayerCorrectChanged(byte oldVal, byte newVal, bool _)
        {
            CorrectGuessChanged?.Invoke(oldVal, newVal);
        }

        void PlayerWrongChanged(byte oldVal, byte newVal, bool _)
        {
            WrongGuessChanged?.Invoke(oldVal, newVal);
        }

        void OnGuessChanged(bool _, bool newVal, bool __)
        {
            GuessChanged?.Invoke(newVal);
        }

        void OnVoteChanged(VoteAnswer oldVal, VoteAnswer newVal, bool _)
        {
            VoteChanged?.Invoke(oldVal, newVal);
        }

        #endregion

        #region Server Part

        /// <summary>
        /// This is invoked for NetworkBehaviour objects when they become active on the server.
        /// <para>This could be triggered by NetworkServer.Listen() for objects in the scene, or by NetworkServer.Spawn() for objects that are dynamically created.</para>
        /// <para>This will be called for objects on a "host" as well as for object on a dedicated server.</para>
        /// </summary>
        public override void OnStartServer()
        {
            base.OnStartServer();
            CorrectCount = WrongCount = 0;
            VoteAnswer = VoteAnswer.None;
        }

        [Server]
        public void IncrementWrong() => WrongCount++;

        [Server]
        public void IncrementCorrect() => CorrectCount++;

        [Server]
        public void SetGuess(bool isGuess) => IsGuessing = isGuess;

        [Server]
        public void SetVote(VoteAnswer vote) => VoteAnswer = vote;

        #endregion

        #region Client Part

        public static int Compare(SETPlayer player1, SETPlayer player2)
        {
            int r = player1.Score.CompareTo(player2.Score);

            if(r == 0)
                return player1.CorrectCount.CompareTo(player2.CorrectCount);

            return r;
        }

        #endregion
    }
}