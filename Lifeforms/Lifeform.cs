﻿using dotSpace.BaseClasses;
using dotSpace.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Lifeforms
{
    public sealed class Lifeform : AgentBase
    {
        private int width;
        private int height;
        private Random rng;

        private int roamX;
        private int roamY;
        private int nrChildren;
        private int maxSpeedgain;
        private int breedingCost;
        private int maxFood;
        private Position targetMate;
        private Food targetFood;

        public Lifeform(SpawnLifeform spawn, int width, int height, ISpace ts) : base(spawn.Genom.ToString(), ts)
        {
            string id = Guid.NewGuid().ToString();
            this.rng = new Random(Environment.TickCount);
            this.nrChildren = 0;
            this.maxSpeedgain = 40;
            this.breedingCost = 50;
            this.maxFood = this.breedingCost * 2;
            this.targetMate = null;
            this.targetFood = null;
            this.width = width;
            this.height = height;
            this.Stats = new LifeformStats(id, this.maxSpeedgain, spawn);
            this.Position = new Position(id, spawn.X, spawn.Y);
            this.FoodAmount = spawn.Food;
        }

        private LifeformStats Stats { get; set; }
        private Position Position { get; set; }
        private int FoodAmount { get; set; }

        protected override void DoWork()
        {
            // Wait until we can start
            this.Query(EntityType.SIGNAL, "start");
            this.Put(this.Position);
            this.Put(this.Stats);
            this.roamX = (this.rng.Next() % (this.width - 3)) + 1;
            this.roamY = (this.rng.Next() % (this.height - 3)) + 1;
            // Keep iterating while the state is 'running'
            while (this.QueryP(EntityType.SIGNAL, "running", true) != null && this.Stats.Life > 0)
            {
                // if the lifeform has more food than the cost to breed, then find a mate
                if (this.FoodAmount > this.breedingCost && nrChildren < this.Stats.MaxNrChildren)
                {
                    // is the mate close by?
                    if (this.targetMate != null && this.IsNearby(this.targetMate.X, this.targetMate.Y))
                    {
                        this.Breed(this.targetMate);
                    }
                    else
                    {
                        // We have no mate, so try to find one.'
                        if (this.targetMate == null)
                        {
                            this.targetMate = this.FindMate();
                        }

                        // if we found one, then move towards it
                        if (this.targetMate != null)
                        {
                            // update the mate's location
                            if ((this.targetMate = (Position)this.QueryP(EntityType.POSITION, this.targetMate.Id, typeof(int), typeof(int))) != null)
                            {
                                this.Move(this.targetMate.X, this.targetMate.Y);
                            }
                        }
                        // otherwise roam
                        else
                        {
                            this.Roam();
                        }
                    }
                }
                // otherwise search for food
                else
                {
                    if (this.targetFood != null && this.IsNearby(this.targetFood.X, this.targetFood.Y))
                    {
                        this.Eat();
                    }
                    else
                    {
                        if (this.targetFood == null)
                        {
                            this.targetFood = this.FindFood();
                        }
                        if (this.targetFood != null)
                        {
                            this.Move(this.targetFood.X, this.targetFood.Y);
                        }
                        // otherwise roam
                        else
                        {
                            this.Roam();
                        }
                    }
                }

                this.FoodAmount = Math.Max(this.FoodAmount - 1, 0);
                if (this.FoodAmount == 0)
                {
                    this.Stats.Life--;
                }
                Thread.Sleep(50 - this.Stats.Speed);
            }
            this.Get(EntityType.POSITION, this.Stats.Id, typeof(int), typeof(int));
            this.Get(EntityType.LIFEFORM_STATS, this.Stats.Id, typeof(long), typeof(long), typeof(long), typeof(int), typeof(int), typeof(int), typeof(int), typeof(int), typeof(int));
        }
        private void Roam()
        {
            if (this.Position.X == this.roamX && this.Position.Y == this.roamY)
            {
                this.roamX = (this.rng.Next() % (this.width - 3)) + 1;
                this.roamY = (this.rng.Next() % (this.height - 3)) + 1;
            }
            this.Move(this.roamX, this.roamY);
        }
        private bool IsNearby(int x, int y)
        {
            return Math.Abs(this.Position.X - x) <= 1 && Math.Abs(this.Position.Y - y) <= 1;
        }
        private void Eat()
        {
            Food food = (Food)this.GetP(EntityType.FOOD, this.targetFood.Amount, typeof(int), this.targetFood.X, this.targetFood.Y);
            if (food != null)
            {
                int foodDiff = this.maxFood - this.FoodAmount;
                if (foodDiff > 0)
                {
                    int eat = Math.Min(foodDiff, food.Amount);
                    this.FoodAmount += eat;
                    food.Amount -= eat;

                    if (food.Amount > 0)
                    {
                        this.Put(food);
                    }
                    else
                    {
                        this.Put(EntityType.SIGNAL, "foodEaten");
                    }
                }
            }
            this.targetFood = null;
        }
        private Position FindMate()
        {
            IEnumerable<Position> targetmates = this.QueryAll(EntityType.POSITION, typeof(string), typeof(int), typeof(int)).Cast<Position>();
            targetmates = targetmates.Where(lf => this.CanSee(lf.X, lf.Y) && this.CanBreed(lf)).ToList();
            if (targetmates.Count() > 0)
            {
                return targetmates.ElementAt(rng.Next() % targetmates.Count());
            }
            return null;
        }
        private Food FindFood()
        {
            IEnumerable<Food> targetFoods = this.QueryAll(EntityType.FOOD, typeof(int), typeof(int), typeof(int), typeof(int)).Cast<Food>();
            targetFoods = targetFoods.Where(f => this.CanSee(f.X, f.Y));
            if (targetFoods.Count() > 0)
            {
                return targetFoods.ElementAt(rng.Next() % targetFoods.Count());
            }
            return null;
        }
        private void Move(int x, int y)
        {
            int deltaX = Math.Abs(this.Position.X - x);
            int deltaY = Math.Abs(this.Position.Y - y);
            this.Get(EntityType.POSITION, this.Stats.Id, typeof(int), typeof(int));

            if (deltaX > deltaY)
            {
                if (this.Position.X > x)
                {
                    this.Position.X--;
                }
                else if (this.Position.X < x)
                {
                    this.Position.X++;
                }
            }
            else
            {
                if (this.Position.Y < y)
                {
                    this.Position.Y++;
                }
                else if (this.Position.Y > y)
                {
                    this.Position.Y--;
                }
            }
            this.Position.X = Math.Min(this.Position.X, this.width - 2);
            this.Position.X = Math.Max(this.Position.X, 1);
            this.Position.Y = Math.Min(this.Position.Y, this.height - 2);
            this.Position.Y = Math.Max(this.Position.Y, 1);
            this.Put(this.Position);
        }
        private bool CanSee(int x, int y)
        {
            int deltaX = Math.Abs(this.Position.X - x);
            int deltaY = Math.Abs(this.Position.Y - y);
            int dist = (int)Math.Sqrt(deltaX * deltaX + deltaY * deltaY);
            return dist <= this.Stats.VisualRange;
        }
        private void GetFreeAdjacentCell(out int x, out int y)
        {
            ITuple adjacent = this.QueryP(EntityType.POSITION, typeof(string), this.Position.X, this.Position.Y + 1);
            if (adjacent == null)
            {
                adjacent = this.QueryP(EntityType.POSITION, typeof(string), this.Position.X + 1, this.Position.Y);
                if (adjacent == null)
                {
                    adjacent = this.QueryP(EntityType.POSITION, typeof(string), this.Position.X - 1, this.Position.Y);
                    if (adjacent == null)
                    {
                        adjacent = this.QueryP(EntityType.POSITION, typeof(string), this.Position.X, this.Position.Y - 1);
                        if (adjacent == null)
                        {
                            x = -1;
                            y = -1;
                        }
                        x = this.Position.X;
                        y = this.Position.Y - 1;
                    }
                    x = this.Position.X;
                    y = this.Position.Y - 1;
                }
                x = this.Position.X + 1;
                y = this.Position.Y;
            }
            x = this.Position.X;
            y = this.Position.Y + 1;
        }
        private bool CanBreed(Position other)
        {
            if (other == null)
            {
                return false;
            }
            LifeformStats stats = (LifeformStats)this.QueryP(EntityType.LIFEFORM_STATS, other.Id, typeof(long), typeof(long), typeof(long), typeof(int), typeof(int), typeof(int), typeof(int), typeof(int), typeof(int));
            if (stats == null)
            {
                return false;
            }

            return (this.Stats.Genom != stats.P1Genom && this.Stats.Genom != stats.P2Genom) &&
                   (this.Stats.P1Genom != stats.P1Genom && this.Stats.P1Genom != stats.P2Genom) &&
                   (this.Stats.P2Genom != stats.P1Genom && this.Stats.P2Genom != stats.P2Genom);
        }
        private void Breed(Position other)
        {
            int x = -1;
            int y = -1;
            this.GetFreeAdjacentCell(out x, out y);
            if (x > 0 && y > 0)
            {
                LifeformStats mate = (LifeformStats)this.QueryP(EntityType.LIFEFORM_STATS, this.targetMate.Id, typeof(long), typeof(long), typeof(long), typeof(int), typeof(int), typeof(int), typeof(int), typeof(int), typeof(int));
                if (mate != null)
                {
                    x = x == this.width ? this.Position.X - 1 : this.Position.X;
                    y = y == this.width ? this.Position.Y - 1 : this.Position.Y;

                    this.Stats.Life -= this.Stats.InitialLife / this.Stats.MaxNrChildren;
                    this.FoodAmount -= this.breedingCost;

                    long genom = 31 + this.Stats.Genom;
                    genom = (genom * 31) + mate.Genom;

                    int initiallife = (this.Stats.InitialLife + mate.InitialLife) / 2;
                    initiallife += (this.rng.Next() % (initiallife / 2)) - (initiallife / 4);

                    int food = 0;
                    int generation = Math.Max(this.Stats.Generation, mate.Generation) + 1;

                    int visualRange = (this.Stats.VisualRange + mate.VisualRange) / 2;
                    visualRange += (this.rng.Next() % visualRange) - ((visualRange - 1) / 2);

                    int maxNrChildren = ((this.Stats.MaxNrChildren + mate.MaxNrChildren) / 2) + (this.rng.Next() % 5) - 2;
                    int speed = Math.Max(this.Stats.Speed, mate.Speed) + (this.rng.Next() % 5) - 2;

                    this.Put(EntityType.SPAWN, genom, this.Stats.Genom, mate.Genom, initiallife, food, x, y, generation, visualRange, maxNrChildren, speed);
                    this.nrChildren++;
                }
            }
            this.targetMate = null;
        }
    }
}
