﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Threading;
using FortuneWheelLibrary;


/*
 * Main lobby Fortune Wheel game
 * Authors: Anthony Merante & James Kidd
 * Date: April 1 - 2021
 */

namespace FortuneWheel
{
    [CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Reentrant, UseSynchronizationContext = false)]
    public partial class MainMenu : Form, ICallback
    {
        private const int MAX_PLAYERS = 4;
        private Dispatcher thread = Dispatcher.CurrentDispatcher;
        private Player user;
        private IWheel wheel;
        private List<Player> players;
        private List<Label> playerLabels;
        private bool GameStarted;
        private GamePanel gamePanel;


        /*                                                                                                                            
           88               88              88b           d88                       88                                 88             
           88               ""    ,d        888b         d888                ,d     88                                 88             
           88                     88        88`8b       d8'88                88     88                                 88             
           88  8b,dPPYba,   88  MM88MMM     88 `8b     d8' 88   ,adPPYba,  MM88MMM  88,dPPYba,    ,adPPYba,    ,adPPYb,88  ,adPPYba,  
           88  88P'   `"8a  88    88        88  `8b   d8'  88  a8P_____88    88     88P'    "8a  a8"     "8a  a8"    `Y88  I8[    ""  
           88  88       88  88    88        88   `8b d8'   88  8PP"""""""    88     88       88  8b       d8  8b       88   `"Y8ba,   
           88  88       88  88    88,       88    `888'    88  "8b,   ,aa    88,    88       88  "8a,   ,a8"  "8a,   ,d88  aa    ]8I  
           88  88       88  88    "Y888     88     `8'     88   `"Ybbd8"'    "Y888  88       88   `"YbbdP"'    `"8bbdP"Y8  `"YbbdP"' 
         */

        public MainMenu()
        {
            players = new List<Player>();
            InitializeComponent();
            playerLabels = new List<Label>();
            playerLabels.Add(label_Player1);
            playerLabels.Add(label_Player2);
            playerLabels.Add(label_Player3);
            playerLabels.Add(label_Player4);
        }


        private delegate void GuiUpdateDelegate(Player[] messages);
        /// <summary>
        /// Callback method that updates the players UI or forwards the callback to the appropriate form
        /// </summary>
        /// <param name="messages">Contains updated player info</param>
        public void PlayersUpdated(Player[] messages)
        {
            if (thread.Thread == Thread.CurrentThread)
            {
                // If the game has started, forward the callback message to the game panels callback handler
                if (GameStarted)
                {
                    BeginInvoke(new GuiUpdateDelegate(gamePanel.PlayersUpdated), new object[] { messages });
                    return;
                }
                // Otherwise update the UI
                try
                {
                    players = messages.ToList();
                    UpdatePlayers();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            else
                BeginInvoke(new GuiUpdateDelegate(PlayersUpdated), new object[] { messages });
        }

        /// <summary>
        /// Updates the players UI when a player joins or ready status changes
        /// When all players are ready (min 2) it will start the game
        /// </summary>
        private void UpdatePlayers()
        {
            int readyPlayers = 0;
            if (players.Count == 0)
            {
                return;
            }
            // update the ui
            for (int i = 0; i <= playerLabels.Count; i++)
            {
                if (players.Count <= i)
                {
                    break;
                }
                string ready = players[i].isReady ? "(Ready)" : "(Not Ready)";
                readyPlayers += players[i].isReady ? 1 : 0;
                playerLabels[i].Text = $@"{players[i]} {ready}";
            }
            // when ready condition is met, start the game
            if (readyPlayers >= 2 && readyPlayers == players.Count)
            {
                GameStarted = true;
                wheel.StartGame();
                Hide();
                gamePanel ??= new GamePanel(wheel, players, user);
                gamePanel.Show();
                gamePanel.FormClosed += (_, _) => wheel.LeaveGame();
                gamePanel.FormClosed += (_, _) => Close();
                gamePanel.PlayersUpdated(players.ToArray());
            }
            
        }

        /*
                                                                                                ,,   ,,                          
        `7MM"""YMM                             mm       `7MMF'  `7MMF'                        `7MM `7MM                          
          MM    `7                             MM         MM      MM                            MM   MM                          
          MM   d `7M'   `MF'.gP"Ya `7MMpMMMb.mmMMmm       MM      MM   ,6"Yb. `7MMpMMMb.   ,M""bMM   MM  .gP"Ya `7Mb,od8 ,pP"Ybd 
          MMmmMM   VA   ,V ,M'   Yb  MM    MM  MM         MMmmmmmmMM  8)   MM   MM    MM ,AP    MM   MM ,M'   Yb  MM' "' 8I   `" 
          MM   Y  , VA ,V  8M""""""  MM    MM  MM         MM      MM   ,pm9MM   MM    MM 8MI    MM   MM 8M""""""  MM     `YMMMa. 
          MM     ,M  VVV   YM.    ,  MM    MM  MM         MM      MM  8M   MM   MM    MM `Mb    MM   MM YM.    ,  MM     L.   I8 
        .JMMmmmmMMM   W     `Mbmmd'.JMML  JMML.`Mbmo    .JMML.  .JMML.`Moo9^Yo.JMML  JMML.`Wbmd"MML.JMML.`Mbmmd'.JMML.   M9mmmP' 
         */

        /// <summary>
        /// Button event handler to send a request for the user to join the game
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_join_Click(object sender, EventArgs e)
        {
            try
            {
                // Create the channel
                DuplexChannelFactory<IWheel> channel = new DuplexChannelFactory<IWheel>(this, "WheelService");
                wheel = channel.CreateChannel();
                // if the user is added update ui
                if (wheel.AddPlayer(textBox_UserName.Text, out user))
                {
                    players = wheel.GetAllPlayers().ToList();
                    UpdatePlayers();
                    button_join.Enabled = false;
                }
                // otherwise show an error message
                else
                {
                    if (wheel.GetAllPlayers().Length == MAX_PLAYERS)
                    {
                        MessageBox.Show(@"ERROR: No room for any additional players");
                    }
                    else if (wheel.GameStarted())
                    {
                        MessageBox.Show(@"ERROR: Game in progress. Please try again later.");
                    }
                    else
                    {
                        MessageBox.Show(@"ERROR: Alias in use. Please try again.");
                    }
                    // Alias rejected by the service so nullify service proxies
                    wheel = null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// Sets player to ready
        /// </summary>
        private void button_Ready_Click(object sender, EventArgs e)
        {
            user.isReady = !user.isReady;
            wheel.UpdatePlayer(user);
            UpdatePlayers();
        }
    }
}
