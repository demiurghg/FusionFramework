using Fusion;
using Fusion.Mathematics;
using Fusion.Graphics;
using SubmarinesWars.SubmarinesGameLibrary.GameEntity;
using SubmarinesWars.SubmarinesGameLibrary.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubmarinesWars.SubmarinesGameLibrary.GameEntity
{
    public class EntityCollection : VisibleObject
    {
        private List<VisibleObject> collection;
        internal List<VisibleObject> Collection
        {
            get { return collection; }
        }

        private Team winner;
        internal Team Winner
        {
            get { return winner; }
        }


        private VisibleObject _parent;
        internal VisibleObject Parent
        {
            get { return _parent; }
            set { _parent = value; }
        }

        public EntityCollection()
        {
            collection = new List<VisibleObject>();
            winner = null;
        }

        internal virtual void addToCollection(VisibleObject obj)
        {
            collection.Add(obj);
        }

        internal override void Remove(VisibleObject obj)
        {
            List<VisibleObject> copy = new List<VisibleObject>();
            copy.AddRange(collection);
            collection.Remove(obj);
            foreach (VisibleObject item in copy)
                item.Remove(obj);
        }

        internal override bool Intersection(VisibleObject obj)
        {
            return false;
        }

        internal virtual void detectNoise()
        {
            foreach (VisibleObject obj in Collection)
                if (obj is Team)
                    ((Team)obj).detectNoise();
        }

        internal override void Update(GameTime gameTime)
        {
            EntityCollection copy = new EntityCollection();
            copy.Collection.AddRange(Collection);
            int aliveTeamCount = 0;
            Team winner = null;
            foreach (VisibleObject obj in copy.Collection)
            {
                obj.Update(gameTime);
                if (obj is Team)
                    if (((Team)obj).Lose == false)
                    {
                        aliveTeamCount++;
                        winner = (Team)obj;
                    }
            }
            if (aliveTeamCount == 1)
                this.winner = winner;
        }

        internal override void Draw(SpriteBatch sb, DebugStrings ds, StereoEye stereoEye)
        {
            for (int i = 0; i < 7; i++)
                Draw(sb, ds, i, stereoEye);
        }

        internal override void Draw(SpriteBatch sb, DebugStrings ds, int order, StereoEye stereoEye)
        {
            foreach (VisibleObject obj in collection)
                obj.Draw(sb, ds, order, stereoEye);
        }

        internal List<Team> getTeams()
        {
            List<Team> teams = new List<Team>();
            foreach (VisibleObject obj in collection)
                if (obj is Team)
                    teams.Add((Team)obj);
            return teams;
        }

        internal virtual List<Submarine> getSubmarines()
        {
            List<Submarine> submarines = new List<Submarine>();
            foreach (VisibleObject obj in collection)
                if (obj is Team)
                    submarines.AddRange(((Team)obj).getSubmarines());
            return submarines;
        }

        internal virtual List<Mine> getMines()
        {
            List<Mine> mines = new List<Mine>();
            foreach (VisibleObject obj in collection)
                if (obj is Team)
                    mines.AddRange(((Team)obj).getMines());
            return mines;
        }


        internal override void GlobalUpdate()
        {
            EntityCollection copy = new EntityCollection();
            copy.Collection.AddRange(Collection);

            foreach (VisibleObject obj in copy.Collection)
                obj.GlobalUpdate();
        }

        internal override void Delay(double delay)
        {
            foreach (VisibleObject obj in Collection)
                obj.Delay(delay);
        }

        internal override VisibleObject Copy(VisibleObject parent)
        {
            EntityCollection newEC = new EntityCollection();
            foreach (VisibleObject obj in Collection)
                newEC.addToCollection(obj.Copy(newEC));
            return newEC;
        }
    }
}
