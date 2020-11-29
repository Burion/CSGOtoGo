using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using DemoInfo;


namespace CSGOtoGo
{
    public partial class Form1 : Form
    {   
        object locker = new object();
        delegate void UpdateTable();
        // const float rightBound = 450;
        // const float leftBound = -4750;
        // const float upperBound = 1650;
        // const float lowerBound = -3450;
        int fps = 50;
        bool isPlaying = true;
        const float rightBound = 2000;
        const float leftBound = -2400;
        const float upperBound = 3200;
        const float lowerBound = -1200;

        const float pwidth = 900f;

        Label l;
        TableLayoutPanel table;
        DataGridView dg;
        DataGridView scoreTable;
        PictureBox p;
        Panel smokesPanel;
        DemoParser parser;
        DemoParser trueParser;

        List<Nade> Smokes;
        List<Player> FlashedP;
        event Action GamePaused;
        event Action GameUnPaused;
        public Form1()
        {
            this.SetStyle(
            ControlStyles.AllPaintingInWmPaint |
            ControlStyles.UserPaint |
            ControlStyles.DoubleBuffer,true);
            l = new Label();
            //Controls.Add(l);
            Smokes = new List<Nade>();
            FlashedP = new List<Player>();
            Button btn = new Button() {Text = "Hit me!", Size = new Size() { Height = 100, Width = 100 } };
            btn.Click += new EventHandler(ButtonClik);
            //Controls.Add(btn);

            table = new TableLayoutPanel() { Width = 600, Height = 400, AutoScroll = true };
            table.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            table.CellBorderStyle = TableLayoutPanelCellBorderStyle.Inset;
                        
            //parser.TickDone += (sender, e) => {table.Controls.Add(new Label() {Text = "tick", Width = 100 }, 0, table.RowCount); table.Update();};
            //Controls.Add(table);
            dg = new DataGridView() { Width = 600, Height = 180, ReadOnly = true, AllowUserToAddRows = false, RowHeadersVisible = false, AllowUserToOrderColumns = false, AllowUserToResizeColumns = false, AllowUserToResizeRows = false};
            dg.ClearSelection();
            dg.SelectionChanged += (o,e) => {dg.ClearSelection();};
            dg.Columns.Add("killer", "Killer");
            dg.Columns.Add("weapon", "Weapon");
            dg.Columns.Add("victim", "Victim");

            scoreTable = new DataGridView() { Location = new Point(0, 180), Width = 600, Height = 800, ReadOnly = true, AllowUserToAddRows = false, RowHeadersVisible = false, AllowUserToOrderColumns = false, AllowUserToResizeColumns = false, AllowUserToResizeRows = false};
            scoreTable.ClearSelection();
            dg.SelectionChanged += (o,e) => {dg.ClearSelection();};
            
            scoreTable.Columns.Add("items", "I");
            scoreTable.Columns.Add("weapon", "Weapon");
            scoreTable.Columns.Add("name", "Name");
            scoreTable.Columns.Add("kills", "Kills");
            scoreTable.Columns.Add("assists", "Assists");
            scoreTable.Columns.Add("deaths", "Deaths");
            scoreTable.Columns.Add("score", "Score");
            scoreTable.Columns[3].Width = 40;
            scoreTable.Columns[4].Width = 40;
            scoreTable.Columns[5].Width = 40;
            scoreTable.Columns[6].Width = 40;

            Controls.Add(scoreTable);

            foreach (DataGridViewColumn column in dg.Columns)
            {
                column.SortMode = DataGridViewColumnSortMode.NotSortable;
            }
            Controls.Add(dg);
            p = new PictureBox() { Name = "Map", Location = new Point(600, 0), SizeMode = PictureBoxSizeMode.StretchImage, Size = new Size() { Width = (int)pwidth, Height = (int)pwidth }, Image = Image.FromFile("Map.jpg") };
            Button stopB = new Button() { Text = "||", Location = new Point(600, 900), Size = new Size(100, 50)};
            Button normalB = new Button() { Text = ">", Location = new Point(700, 900), Size = new Size(100, 50)};
            Button speedB = new Button() { Text = ">>", Location = new Point(800, 900), Size = new Size(100, 50)};
            ComboBox box = new ComboBox() { Width = 100, Height = 50, Location = new Point(1600, 0), DataSource =
                new int[] {
                1,2,3,4,5,6,7,8,9,10,11,12,13,14,15
                },
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            box.SelectionChangeCommitted += (o,e) => {isPlaying = false; Thread.Sleep(100); while(parser.TScore + parser.CTScore != (int)box.SelectedValue - 1) { parser.ParseNextTick(); } isPlaying = true;};
            normalB.Click += NormalSpeedButton;
            speedB.Click += IncreaseSpeedButton;
            stopB.Click += StopSpeedButton;
            Controls.Add(p);
            Controls.Add(normalB);
            Controls.Add(speedB);
            Controls.Add(stopB);
            Controls.Add(box);
            InitializeComponent();
            
            GamePaused += () => 
            { 
                Monitor.Enter(locker);
                trueParser = parser; 
                parser = new IdleDemoParser(File.OpenRead("test3.dem")); 
                parser.ParseHeader(); 
                isPlaying = false; 
                Monitor.Exit(locker);
            };
            GameUnPaused += () => { parser = trueParser; isPlaying = true;};
 
            ChangeLabel();

        }


        void UpdateScoreTable(params Player[] players)
        {
                const string bomb = "🖩";
                const string armor = "🛡";
                const string helmet = "⛑";
                const string kits = "✄";
            
            if(players.Count() == 0)
            {
                scoreTable.Rows.Clear();
                foreach(var g in parser.PlayingParticipants.GroupBy(p => p.Team))
                {
                    foreach(var p in g)
                    {
                        string items = "";
                        items += p.Armor > 0 ? armor : "";
                        items += p.HasHelmet ? helmet : "";
                        items += p.HasDefuseKit ? kits : "";
                        items += p.Weapons.Select(w => w.Weapon.ToString()).Contains("Bomb") ? bomb : "";
                        var r = scoreTable.Rows.Add(items, p.ActiveWeapon?.Weapon, p.Name, p.AdditionaInformations?.Kills, p.AdditionaInformations?.Assists, p.AdditionaInformations?.Deaths, p.AdditionaInformations?.Score);
                        scoreTable.Rows[r].DefaultCellStyle.BackColor = p.Team == Team.CounterTerrorist ? Color.Blue : Color.Orange;
                    }
                }
                return;
            }

            foreach(var p in players)
            {
                for(int x = 0; x < scoreTable.Rows.Count; x ++)
                {
                    if(scoreTable.Rows[x].Cells[2].Value.ToString() == p.Name)
                    {
                        string items = "";
                        items += p.Armor > 0 ? armor : "";
                        items += p.HasHelmet ? helmet : "";
                        items += p.HasDefuseKit ? kits : "";
                        items += p.Weapons.Select(w => w.Weapon.ToString()).Contains("Bomb") ? bomb : "";
                        scoreTable.Rows[x].Cells[0].Value = items;
                        scoreTable.Rows[x].Cells[1].Value = p.ActiveWeapon?.Weapon;
                        scoreTable.Rows[x].Cells[2].Value = p.Name;
                        scoreTable.Rows[x].Cells[3].Value = p.AdditionaInformations?.Kills;
                        scoreTable.Rows[x].Cells[4].Value = p.AdditionaInformations?.Assists;
                        scoreTable.Rows[x].Cells[5].Value = p.AdditionaInformations?.Deaths;
                        scoreTable.Rows[x].Cells[6].Value = p.AdditionaInformations?.Score;
                    }
                }
                
            }

            
        }
        public void IncreaseSpeedButton(object o, EventArgs e)
        {
            fps = 5;
        }
        public void NormalSpeedButton(object o, EventArgs e)
        {
            fps = 30;
        }
        public void StopSpeedButton(object o, EventArgs e)
        {
            isPlaying = !isPlaying;
            ((Button)o).Text = !isPlaying ? "▷" : "||"; 
        }
        public void DrawEllipseRectangle(PaintEventArgs e)
        {           
            // Create pen.
            Pen blackPen = new Pen(Color.Black, 3);
                    
            // Create rectangle.
            Rectangle rect = new Rectangle(0, 0, 200, 200);
                    
            // Draw rectangle to screen.
            e.Graphics.DrawRectangle(blackPen, rect);
        }
        
        async void ChangeLabel()
        {   

            p.Paint += (o, e) => {
                Graphics g = e.Graphics;
                
                using (SolidBrush selPen = new SolidBrush(Color.Blue))
                {   
                    foreach(var p in parser.Participants)
                    {
                        if(p.Team != Team.Spectate && p.IsAlive)
                        {
                            float x = ((4400f - (rightBound - p.Position.X))/10f)/440f*pwidth;
                            float y = ((upperBound - p.Position.Y)/10f)/440f*pwidth;
                            selPen.Color = p.Team == Team.CounterTerrorist ? Color.Blue : Color.Orange;
                            selPen.Color = FlashedP.Select(pl => pl.Name).Contains(p.Name) ? Color.White : selPen.Color;
                            g.FillEllipse(selPen, x - 5, y - 5, 10f, 10f);
                            g.FillPolygon(selPen, new PointF[] {new PointF(x - 5f*(float)Math.Cos((p.ViewDirectionX + 90)/180*Math.PI), y + 5f*(float)Math.Sin((p.ViewDirectionX + 90)/180*Math.PI)), new PointF(x + 5f*(float)Math.Cos((p.ViewDirectionX + 90)/180*Math.PI), y - 5f*(float)Math.Sin((p.ViewDirectionX + 90)/180*Math.PI)), new PointF(x + 10f*(float)Math.Sin((p.ViewDirectionX + 90)/180*Math.PI), y + 10f*(float)Math.Cos((p.ViewDirectionX + 90)/180*Math.PI))});
                            g.DrawString(p.Name, new Font("Arial", 8), selPen, x, y + 8, new StringFormat() { Alignment = StringAlignment.Center });
                        }
                    } 

                }

                using (SolidBrush smokeBrush = new SolidBrush(Color.Gray))
                {   
                    foreach(var s in Smokes)
                    {
                        float x = ((4400f - (rightBound - s.X))/10f)/440f*pwidth;
                        float y = ((upperBound - s.Y)/10f)/440f*pwidth;    
                        g.FillEllipse(smokeBrush, x - 25, y + 25, 50f, 50f);
                    }
                    
                }
            };
            //Invalidate();
            
            parser = new DemoParser(File.OpenRead("test3.dem"));
            parser.ParseHeader();
            parser.TickDone += (o, e) => p.Invalidate();
            parser.RoundEnd += (o, e) => 
            {
                Smokes.Clear();
            };
            parser.SmokeNadeStarted += (o, e) => 
            {
                Smokes.Add(new Nade() { X = e.Position.X, Y = e.Position.Y, ThrownBy = e.ThrownBy.Name });
            };

            parser.SmokeNadeEnded += (o, e) => 
            {
                Smokes.Remove(Smokes.Single(s => s.X == e.Position.X && s.Y == e.Position.Y));
            };

            parser.FlashNadeExploded += (o, e) => 
            {
                FlashedP.AddRange(e.FlashedPlayers);
                Task.Run(() => { Thread.Sleep(fps*100); FlashedP.RemoveAll(pl => e.FlashedPlayers.Contains(pl));});
            };
            parser.RoundStart += (o,e) => { UpdateScoreTable();};
            parser.TickDone += (o,e) => { UpdateScoreTable(parser.PlayingParticipants.ToArray());};
            parser.PlayerKilled += (sender, e) => { 
                if(dg.Rows.Count >= 5)
                {
                    dg.Rows.RemoveAt(0);
                }
                dg.Rows.Add(e.Killer.Name,  (e.Headshot ? "🗣 " : "") + e.Weapon?.Weapon.ToString(), e.Victim?.Name);
                UpdateScoreTable(e.Killer, e.Victim);
            };   
                
            
            await Task.Run(() => { while(parser.ParseNextTick()){ Thread.Sleep(fps); while(!isPlaying) {Thread.Sleep(1);} }; } );
        }
        void ButtonClik(object sender, EventArgs e)
        {
            ChangeLabel();
        }
        void UpdateForm()
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke((MethodInvoker)delegate()
                {
                    Task t = new Task(() => { for(int x = 0; x < 10; x++) { table.Controls.Add(new Label() {Text = DateTime.Now.ToString(), Width = 50 }, 0, table.RowCount); System.Threading.Thread.Sleep(1000); } table.Update(); });
                    t.Start();
                    t.Wait();
                });
            }
        }

    }
}
