using Common;
using Manager.Manager;
using Manager.Status.Server;
using Manager.StatusHook;
using Parser;
using Parser.Parsers;
using System.Diagnostics;
using System.Threading;

namespace Manager
{
    public partial class Main : Form
    {
        public static ManagerClient ManagerClient { get; } = new ManagerClient();
        public static List<Pair<ClientConfigLine, EntityStatus>> Clients { get; private set; } = new List<Pair<ClientConfigLine, EntityStatus>>();
        public static List<Pair<ServerConfigLine, EntityStatus>> TransactionManagers { get; private set; } = new List<Pair<ServerConfigLine, EntityStatus>>();
        public static List<Pair<ServerConfigLine, EntityStatus>> LeaseManagers { get; private set; } = new List<Pair<ServerConfigLine, EntityStatus>>();

        private ConfigParser? config = null;
        private System.Threading.Timer? processTimer = null;

        public Main()
        {
            InitializeComponent();
        }

        private void Entry_Load(object sender, EventArgs e)
        {
            // Set labels
            this.lblClientsLabel.Text = "Gray = Not Started\nYellow = Idle\nGreen = Sending requests";
            this.lblTMsLabel.Text = "Gray = Not Started\nYellow = Idle\nRed = Crashed\nLight Green = Waiting for Lease\nDark Green = Executing Transaction";
            this.lblLMsLabel.Text = "Gray = Not Started\nYellow = Idle\nRed = Crashed\nLight Green = Paxos Proposer\nDark Green = Paxos Acceptor";

            this.fdConfig.Title = "Select the configuration file";
            this.fdConfig.FileName = "config.txt"; // Default name
            this.fdConfig.ShowDialog();
        }

        private void fdConfig_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                String filePath = this.fdConfig.FileName;
                this.loadConfig(filePath);
                this.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                Environment.Exit(1);
            }
        }

        private void loadConfig(String configFilePath)
        {
            this.config = new ConfigParser(configFilePath);
            this.config.Parse();

            lock (Main.Clients)
            {
                Main.Clients = this.config.Config[ConfigType.Client]
                    .Select(x => new Pair<ClientConfigLine, EntityStatus>((ClientConfigLine)x, new EntityStatus { Status = ClientStatus.NotStarted }))
                    .ToList();
            }

            lock (Main.TransactionManagers)
            {
                Main.TransactionManagers = this.config.Config[ConfigType.Server]
                    .Select(x => new Pair<ServerConfigLine, EntityStatus>((ServerConfigLine)x, new EntityStatus { Status = TMStatus.NotStarted }))
                                   .Where(server => server.First.Type == ServerType.TransactionManager)
                                   .ToList();
            }

            lock (Main.LeaseManagers)
            {
                Main.LeaseManagers = this.config.Config[ConfigType.Server]
                    .Select(x => new Pair<ServerConfigLine, EntityStatus>((ServerConfigLine)x, new EntityStatus { Status = LMStatus.NotStarted }))
                                .Where(server => server.First.Type == ServerType.LeaseManager)
                                .ToList();
            }

            Program.Servers = Main.TransactionManagers.Select(x => new Peer(x.First.ID, x.First.Url)).Concat(
                                    Main.LeaseManagers.Select(x => new Peer(x.First.ID, x.First.Url))).ToList();

            this.startProcessMonitoring();
            this.startProcesses();
        }

        private void startProcessMonitoring()
        {
            // Update GUI every second
            this.processTimer = new System.Threading.Timer(this.updateStatuses!, null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
        }

        private void updateStatuses(object state)
        {
            // Performance could be improved by only updating the items that changed when they changed
            // But this works fine for the time being

            // Clients
            this.lsvwClients.Invoke((MethodInvoker)delegate
            {
                this.lsvwClients.BeginUpdate();
                if (this.lsvwClients.Items.Count != Main.Clients.Count)
                {
                    this.lsvwClients.Items.Clear();
                    this.lsvwClients.Items.AddRange(Main.Clients.Select(client => new ListViewItem()
                    {
                        Text = client.First.ID,
                        ToolTipText = client.First.ScriptPath,
                        BackColor = client.Second.Status,
                    }).ToArray());
                }
                else
                {
                    for (int i = 0; i < this.lsvwClients.Items.Count; i++)
                    {
                        this.lsvwClients.Items[i].BackColor = Main.Clients[i].Second.Status;
                    }
                }
                this.lsvwClients.EndUpdate();
            });

            // TMs
            this.lsvwTMs.Invoke((MethodInvoker)delegate
            {
                this.lsvwTMs.BeginUpdate();
                if (this.lsvwTMs.Items.Count != Main.TransactionManagers.Count)
                {
                    this.lsvwTMs.Items.Clear();
                    this.lsvwTMs.Items.AddRange(TransactionManagers.Select(server => new ListViewItem()
                    {
                        Text = server.First.ID,
                        ToolTipText = server.First.Url,
                        BackColor = server.Second.Status,
                    }).ToArray());
                }
                else
                {
                    for (int i = 0; i < this.lsvwTMs.Items.Count; i++)
                    {
                        this.lsvwTMs.Items[i].BackColor = Main.TransactionManagers[i].Second.Status;
                    }
                }
                this.lsvwTMs.EndUpdate();
            });

            // LMs
            this.lsvwLMs.Invoke((MethodInvoker)delegate
            {
                this.lsvwLMs.BeginUpdate();
                if (this.lsvwLMs.Items.Count != Main.LeaseManagers.Count)
                {
                    this.lsvwLMs.Items.Clear();
                    this.lsvwLMs.Items.AddRange(Main.LeaseManagers.Select(server => new ListViewItem()
                    {
                        Text = server.First.ID,
                        ToolTipText = server.First.Url,
                        BackColor = server.Second.Status,
                    }).ToArray());
                }
                else
                {
                    for (int i = 0; i < this.lsvwLMs.Items.Count; i++)
                    {
                        this.lsvwLMs.Items[i].BackColor = Main.LeaseManagers[i].Second.Status;
                    }
                }
                this.lsvwLMs.EndUpdate();
            });
        }


        private void stopProcessMonitoring()
        {
            // TODO notify peers to stop sending status updates
            this.processTimer?.Dispose();
            this.processTimer = null;
        }

        private void startProcesses()
        {
            string solutionDirectory = this.getSolutionDirectory();

            int slots = ((SlotsConfigLine)this.config!.Config[ConfigType.Slots][0]).Count;
            int slotDuration = ((SlotDurationConfigLine)this.config.Config[ConfigType.SlotDuration][0]).Duration;
            // Start LMs
            foreach (Pair<ServerConfigLine, EntityStatus> lm in Main.LeaseManagers)
            {
                string arguments = $"{Program.ManagerAddress} {lm.First.ID} {lm.First.Url} {slots} {slotDuration}";
                lm.Second.Process = Process.Start(solutionDirectory + "/LeaseManager/bin/Debug/net6.0/LeaseManager.exe", arguments);
            }

            // Start TMs
            foreach (Pair<ServerConfigLine, EntityStatus> tm in Main.TransactionManagers)
            {
                string arguments = $"{Program.ManagerAddress} {tm.First.ID} {tm.First.Url}";
                tm.Second.Process = Process.Start(solutionDirectory + "/TransactionManager/bin/Debug/net6.0/TransactionManager.exe", arguments);
            }

            Thread thread = new Thread(() =>
            {
                // Wait for all LM and TM processes to start
                bool allLMsStarted = false;
                bool allTMsStarted = false;
                while (true)
                {
                    if (Main.LeaseManagers.All(lm => lm.Second.Status == LMStatus.Idle))
                        allLMsStarted = true;

                    if (Main.TransactionManagers.All(tm => tm.Second.Status == TMStatus.Idle))
                        allTMsStarted = true;

                    if (allLMsStarted && allTMsStarted)
                        break;

                    Thread.Sleep(100);
                }

                // Tell all LMs to start Paxos
                List<Peer> lmAddresses = Main.LeaseManagers.Select(lm => new Peer(lm.First.ID, lm.First.Url)).ToList();
                List<Peer> tmAddresses = Main.TransactionManagers.Select(tm => new Peer(tm.First.ID, tm.First.Url)).ToList();
                for (int i = 0; i < Main.LeaseManagers.Count; i++)
                {
                    Logger.GetInstance().Log("Main", $"Starting LM {Main.LeaseManagers[i].First.ID}");
                    Main.ManagerClient.StartLeaseManager(Main.LeaseManagers[i].First.Url, lmAddresses, tmAddresses, i + 1);
                }

                // Tell all TMs to start receiving requests
                for (int i = 0; i < Main.TransactionManagers.Count; i++)
                {
                    Logger.GetInstance().Log("Main", $"Starting TM {Main.TransactionManagers[i].First.ID}");
                    Main.ManagerClient.StartTransactionManager(Main.TransactionManagers[i].First.Url, lmAddresses, tmAddresses);
                }

                // Start Clients
                for (int i = 0; i < Main.Clients.Count; i++)
                {
                    Pair<ClientConfigLine, EntityStatus> client = Main.Clients[i];
                    Logger.GetInstance().Log("Main", $"Starting Client {client.First.ID}");

                    string arguments = $"{client.First.ID} {client.First.ScriptPath} {i % Main.TransactionManagers.Count} {string.Join(" ", Main.TransactionManagers.Select(tm => new Peer(tm.First.ID, tm.First.Url).FullRepresentation()))}";
                    client.Second.Process = Process.Start(solutionDirectory + "/Client/bin/Debug/net6.0/Client.exe", arguments);
                    client.Second.Status = ClientStatus.Idle;
                }
            });

            // Start but don't join so we don't block
            thread.Start();

            // Focus the manager
            // Doesn't work but its fine
            this.BringToFront();
        }

        private string getSolutionDirectory()
        {
            // Adapted from https://stackoverflow.com/a/11882118
            string workingDirectory = Environment.CurrentDirectory;
            string solutionDirectory = Directory.GetParent(workingDirectory).Parent.Parent.Parent.FullName;

            // Check if its the solution or if the project was open some other way
            do
            {
                string[] projects = new string[] { "Client", "Common", "DADTKV", "LeaseManager", "Manager", "ManagerClientServices", "ConfigParser", "TransactionManager" };
                if (projects.All(project => Directory.Exists(solutionDirectory + "/" + project)))
                    break;

                // If its not the correct directory, ask the user to select it or cancel
                DialogResult result = MessageBox.Show("The Manager.exe executable was not started from inside the Visual Studio solution.\nPlease select the solution directory, where the .sln file resides.", "Invalid Solution Directory", MessageBoxButtons.RetryCancel);
                if (result == DialogResult.Cancel)
                    Environment.Exit(0);

                // If the user selected retry, ask for the directory again
                FolderBrowserDialog fbd = new FolderBrowserDialog();
                fbd.Description = "Select the solution directory";
                fbd.ShowDialog();
                solutionDirectory = fbd.SelectedPath;
            } while (true);

            return solutionDirectory;
        }

        private void lsvwLMs_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                ListViewItem focusedItem = lsvwLMs.FocusedItem;
                if (focusedItem != null && focusedItem.Bounds.Contains(e.Location))
                {
                    this.ctxLMs.Show(Cursor.Position);
                }
            }
        }

        private void crashToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ListViewItem focusedItem = lsvwLMs.FocusedItem;
            if (focusedItem == null) return;

            string id = focusedItem.Text;
            Pair<ServerConfigLine, EntityStatus> lm = Main.LeaseManagers.Find(lm => lm.First.ID == id)!;
            if (lm == null) return;

            Main.ManagerClient.Crash(lm.First.Url);
            lm.Second.Status = LMStatus.Crashed;
        }

        private void btnKillAllClick(object sender, EventArgs e)
        {
            // Kill from top to bottom
            foreach (var lm in Main.LeaseManagers)
            {
                lm.Second.Process.Kill();
                lm.Second.Process.Close();
                lm.Second.Process.Dispose();
            }

            foreach (var tm in Main.TransactionManagers)
            {
                tm.Second.Process.Kill();
                tm.Second.Process.Close();
                tm.Second.Process.Dispose();
            }

            foreach (var client in Main.Clients)
            {
                client.Second.Process.Kill();
                client.Second.Process.Close();
                client.Second.Process.Dispose();
            }

            // Kill ourselves 💀
            this.Close();
            this.Dispose();
            Environment.Exit(0);
        }
    }

    public struct EntityStatus
    {
        public Color Status { get; set; }
        public Process Process { get; set; }
    }

    class ClientStatus
    {
        public static readonly Color NotStarted = Color.Gray;
        public static readonly Color Idle = Color.Yellow;
        public static readonly Color SendingRequests = Color.Green;

        public static readonly string[] Statuses = new string[] { "NotStarted", "Idle", "SendingRequests" };
    }

    class TMStatus
    {
        public static readonly Color NotStarted = Color.Gray;
        public static readonly Color Idle = Color.Yellow;
        public static readonly Color Crashed = Color.Red;
        public static readonly Color WaitingLease = Color.LightGreen;
        public static readonly Color ExecutingTransaction = Color.DarkGreen;

        public static readonly string[] Statuses = new string[] { "NotStarted", "Idle", "Crashed", "WaitingLease", "ExecutingTransaction" };
    }

    class LMStatus
    {
        public static readonly Color NotStarted = Color.Gray;
        public static readonly Color Idle = Color.Yellow;
        public static readonly Color Crashed = Color.Red;
        public static readonly Color PaxosProposer = Color.LightGreen;
        public static readonly Color PaxosAcceptor = Color.DarkGreen;

        public static readonly string[] Statuses = new string[] { "NotStarted", "Idle", "Crashed", "PaxosProposer", "PaxosAcceptor" };
    }

}