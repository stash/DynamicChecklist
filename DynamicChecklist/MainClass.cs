using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using DynamicChecklist.ObjectLists;
using StardewValley.Locations;
using QuickGraph;
using QuickGraph.Algorithms;
using Microsoft.Xna.Framework;
using QuickGraph.Graphviz;
using QuickGraph.Graphviz.Dot;

namespace DynamicChecklist
{

    public class MainClass : Mod
    {
        // TODO Idea: Mod which notifies the player when they pick up an item which is in a collection
        public ObjectCollection objectCollection;
        public Keys OpenMenuKey = Keys.NumPad1;
        private ModConfig Config;
        private Texture2D cropsTexture;
        private IModHelper helper;
        private List<ObjectList> objectLists = new List<ObjectList>();

        public override void Entry(IModHelper helper)
        {           
            this.helper = helper;
            this.Config = helper.ReadConfig<ModConfig>();          
            // Menu Events
            MenuEvents.MenuChanged += MenuChangedEvent;
            ControlEvents.KeyPressed += this.ReceiveKeyPress;
            SaveEvents.AfterLoad += this.GameLoadedEvent;
            GameEvents.GameLoaded += this.onGameLoaded;
            TimeEvents.DayOfMonthChanged += this.OnDayOfMonthChanged;
            GraphicsEvents.OnPreRenderHudEvent += this.drawTick;
            try
            {
                OpenMenuKey = (Keys)Enum.Parse(typeof(Keys), Config.OpenMenuKey);
            }
            catch
            {
                // use default value
            }

            var edge = new Edge<string>("a", "a");
        }
        private void drawTick(object sender, EventArgs e)
        {
            if (Game1.currentLocation == null || Game1.gameMode == 11 || Game1.currentMinigame != null || Game1.showingEndOfNightStuff || Game1.gameMode == 6 || Game1.gameMode == 0 || Game1.menuUp || Game1.activeClickableMenu != null)
            {
                return;
            }

            foreach (ObjectList ol in objectLists)
            {
                ol.BeforeDraw();
                ol.Draw(Game1.spriteBatch);
            }
        }
        private void OnDayOfMonthChanged(object sender, EventArgs e)
        {
            foreach (ObjectList ol in objectLists)
            {
                ol.OnNewDay();
            }
        }
        private void showTaskDoneMessage(object sender, EventArgs e)
        {
            var s = (ObjectList)sender;
            Game1.showGlobalMessage(s.TaskDoneMessage);
        }
        private void onGameLoaded(object sender, EventArgs e)
        {
            OverlayTextures.loadTextures(helper.DirectoryPath);
            cropsTexture = loadTexture("Crops.png");          
        }
        private void initializeObjectLists()
        {
            objectLists.Add(new AnimalList(AnimalList.Action.Pet));
            objectLists.Add(new AnimalList(AnimalList.Action.Milk));
            objectLists.Add(new CrabPotList());
            objectLists.Add(new HayList());

            foreach (ObjectList o in objectLists)
            {
                o.TaskFinished += new EventHandler(showTaskDoneMessage);
            }
        }
        private Texture2D loadTexture(String texName)
        {
            var textureStream = new FileStream(Path.Combine(Helper.DirectoryPath, "Resources", texName), FileMode.Open);
            var t = Texture2D.FromStream(Game1.graphics.GraphicsDevice, textureStream);
            return t;
        }
        private void ReceiveKeyPress(object sender, EventArgsKeyPressed e)
        {
            //TODO ignore close menu when entering checkbox name
            if (e.KeyPressed != OpenMenuKey) return;
            if (Game1.activeClickableMenu is ChecklistMenu)
            {
                Game1.activeClickableMenu = null;
            }
            else
            {
                objectCollection.update();
                ChecklistMenu.objectCollection = objectCollection;
                ChecklistMenu.objectLists = objectLists;
                ChecklistMenu.Open();
            }

        }
        public void MenuChangedEvent(object sender, EventArgsClickableMenuChanged e)
        {
            if (!(e.NewMenu is GameMenu))
            {
                return;
            }
            var gameMenu = e.NewMenu;

        }
        private void GameLoadedEvent(object sender, EventArgs e)
        {
            objectCollection = new ObjectCollection(cropsTexture);
            objectCollection.update();
            initializeObjectLists();
            foreach(ObjectList ol in objectLists)
            {
                ol.OnNewDay();
            }
            var a = Game1.currentLocation.warps;
          
            foreach(GameLocation loc in Game1.locations)
            {
                //graph.AddVertex(loc.Name);
            }
            var partialGraphs = new List<AdjacencyGraph<ExtendedWarp, LabelledEdge<ExtendedWarp>>>();
            //var edgeCosts = new List<Dictionary<Edge<ExtendedWarp>, double>>();
            var edgeCost = new Dictionary<LabelledEdge<ExtendedWarp>, double>();
            foreach (GameLocation loc in Game1.locations)
            {
                var partialGraph = new AdjacencyGraph<ExtendedWarp, LabelledEdge<ExtendedWarp>>();
                var warpsToInclude = new List<Warp>();
                var extWarpsToInclude = new List<ExtendedWarp>();
                for (int i = 0; i < loc.warps.Count; i++)
                {
                    bool shouldAdd = true;

                    extWarpsToInclude.Add(new ExtendedWarp(loc.warps.ElementAt(i), loc));
                    partialGraph.AddVertex(extWarpsToInclude.Last());
                }
                for (int i = 0; i < loc.warps.Count; i++) 
                {
                    var extWarp1 = extWarpsToInclude.ElementAt(i);


                    for (int j = i+1; j < loc.warps.Count; j++)
                    {
                        // TODO Dont add adjacant warp tiles or make Extended warp be combined from many warps
                        var LocTo = Game1.getLocationFromName(loc.warps.ElementAt(j).TargetName);
                        var extWarp2 = extWarpsToInclude.ElementAt(j);
                        var edge = new LabelledEdge<ExtendedWarp>(extWarp1, extWarp2, extWarp1.Label);
                        var dist = Vector2.Distance(new Vector2(extWarp1.X, extWarp1.Y), new Vector2(extWarp2.X, extWarp2.Y));
                        partialGraph.AddEdge(edge);
                        edgeCost.Add(edge, dist);
                        
                    }                   
                    partialGraph.AddVertex(extWarp1);                 
                }
                partialGraphs.Add(partialGraph);
            }
            // Combine partial graphs into one
            var wholeGraph = new MyGraph();
            foreach (var partialGraph in partialGraphs)
            {
                wholeGraph.AddVertexRange(partialGraph.Vertices);
                wholeGraph.AddEdgeRange(partialGraph.Edges);
            }
            for (int i=0; i< partialGraphs.Count; i++)
            {
                var graph1 = partialGraphs.ElementAt(i);
                for (int j=i+1; j < partialGraphs.Count; j++)
                {                    
                    var graph2 = partialGraphs.ElementAt(j);
                    foreach(ExtendedWarp warp1 in graph1.Vertices)
                    {
                        foreach(ExtendedWarp warp2 in graph2.Vertices)
                        {
                            if(ExtendedWarp.AreCorresponding(warp1, warp2))
                            {
                                var edge = new LabelledEdge<ExtendedWarp>(warp1, warp2, warp1.Label);
                                wholeGraph.AddEdge(edge);
                                edgeCost.Add(edge, 0);
                            }
                        }
                    }
                }
            }
            GraphvizAlgorithm<ExtendedWarp, LabelledEdge<ExtendedWarp>> graphviz = new GraphvizAlgorithm<ExtendedWarp, LabelledEdge<ExtendedWarp>>(wholeGraph);
            graphviz.FormatVertex += (sender2, args) => args.VertexFormatter.Comment = args.Vertex.Label;
            graphviz.FormatEdge += (sender2, args) => { args.EdgeFormatter.Label.Value = args.Edge.Label; };
            graphviz.ImageType = GraphvizImageType.Jpeg;

            graphviz.Generate(new FileDotEngine(), "C:\\Users\\Gunnar\\Desktop\\graph.png");
            

            //var alg = new QuickGraph.Algorithms.ShortestPath.UndirectedDijkstraShortestPathAlgorithm<ExtendedWarp, Edge<ExtendedWarp>>;
        }
    }

    internal class ModConfig
    {
        public bool val { get; set; } = false;
        public String OpenMenuKey = "NumPad1";
    }
    public class LabelledEdge<TVertex> : Edge<TVertex>
    {
        public string Label;
        public LabelledEdge(TVertex source, TVertex target, string label) : base(source, target)
        {
            this.Label = label;
        }
    }
    public class ExtendedWarp : Warp
    {
        public GameLocation OriginLocation;
        public GameLocation TargetLocation;
        public string Label;

        public ExtendedWarp(Warp w, GameLocation originLocation): base(w.X, w.Y, w.TargetName, w.TargetX, w.TargetY, false)
        {
            this.OriginLocation = originLocation;
            TargetLocation = Game1.getLocationFromName(w.TargetName);
            this.Label = w.TargetName + " to " + originLocation.name;
        }

        public static bool AreCorresponding(ExtendedWarp warp1, ExtendedWarp warp2)
        {
            if (warp1.OriginLocation == warp2.TargetLocation && warp1.TargetLocation == warp2.OriginLocation)
            {
                if(Math.Abs(warp1.X - warp2.TargetX)<=2 && Math.Abs(warp1.Y - warp2.TargetY)<=2)
                {
                    return true;
                }
            }
            return false;
        } 
    }

    public class FileDotEngine : IDotEngine
    {
        public string Run(GraphvizImageType imageType, string dot, string outputFileName)
        {
            //using (StreamWriter writer = new StreamWriter(outputFileName))
            //{
            //    writer.Write(dot);
            //}

            //return System.IO.Path.GetFileName(outputFileName);

            string output = outputFileName;
            File.WriteAllText(output, dot);

            // assumes dot.exe is on the path:
            var args = string.Format(@"{0} -Tjpg -O", output);
            System.Diagnostics.Process.Start(@"C:\Users\Gunnar\Desktop\release\bin\dot.exe", args);
            return output;
        }
    }
    public class MyGraph : AdjacencyGraph<ExtendedWarp, LabelledEdge<ExtendedWarp>>, IEdgeListGraph<ExtendedWarp, LabelledEdge<ExtendedWarp>>
    {
        public MyGraph() : base(false)
        {

        }
    }
}
