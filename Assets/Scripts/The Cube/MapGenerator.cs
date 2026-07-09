using System;
using UnityEngine;
using System.Collections.Generic;

public class Cube
{
    private int size;
    private Level[] levels;
    private int startIndex;
    private int endIndex;

    public Cube(int size)
    {
        if (size < 4)
        {
            size = 4;
        }

        this.size = size;
        levels = new Level[size];

        for ( int i = 0; i < size; i++)
        {
            levels[i] = new Level(i, (int)Math.Pow(size, 2.0f), size);
        }
    }

    public void randomizeStart()
    {
        startIndex = UnityEngine.Random.Range(0, (int)Math.Pow(size, 2.0f));
    }

    public void createThresholdPath()
    {
        int former = startIndex;

        for (int i = size - 1; i >= 0; i--)
        {
            former = levels[i].generateThreshold(former);
        }

        endIndex = former;
    }

    public void calculateDifficulty()
    {
        foreach(Level level in levels)
        {
            level.calculateDifficulty();
        }
    }

    public void handleTypes()
    {
        foreach ( Level level in levels)
        {
            level.addChamberType(level.getDrop(),THRESHOLD_DROP);
            level.addChamberType(level.getThreshold(), THRESHOLD);

            level.setRoomDifficulty(level.getDrop(), 0.0f, ROOM_DIFFICULTY.NOT_APPLYABLE);
            level.setRoomDifficulty(level.getThreshold(), 0.0f, ROOM_DIFFICULTY.NOT_APPLYABLE);

            if (level.getLevel() == size - 1)
            {
                level.addChamberType(level.getDrop(), START);
            }

            if (level.getLevel() == 0)
            {
                level.addChamberType(level.getThreshold(), END);
            }
        }
    }

    public void debugPrint()
    {
        foreach (Level level in levels)
        {
            level.debugPrint();
        }
    }

    public struct RoomInfo
    {
        public int level, x, y, type;
        public float difficulty;
        public int difficultyCategory;   // rzutowany ROOM_DIFFICULTY
    }

    public List<RoomInfo> getRooms()
    {
        var list = new List<RoomInfo>();

        foreach(Level level in levels)
        {
            level.collectRooms(list);
        }

        return list;
    }

    /*
     *  Room type bit mask, allows one room to have multiple types at the same time
     */

    private const int ROOM              = 0b0000_0000_0000_0001;
    private const int START             = 0b0000_0000_0000_0010;
    private const int THRESHOLD         = 0b0000_0000_0000_0100;
    private const int THRESHOLD_DROP    = 0b0000_0000_0000_1000;
    private const int END               = 0b0000_0000_0001_0000;

    private const int DISCOVERED        = 0b0000_0000_0010_0000;
    private const int CHOSEN            = 0b0000_0000_0100_0000;
    private const int COMPLETE          = 0b0000_0000_1000_0000;
    private const int ACTIVE            = 0b0000_0001_0000_0000;

    /*
     *  Room difficulties:
     *  Easy, Medium, Hard  - i think the names describe the difficulty perfectly
     *  Diabolical          - straight up torture, no way to get out unless you lose something
     *  Chaos               - can be good, or can be bad. An example of Chaos chamber is The Prozac Trial
     */
    private enum ROOM_DIFFICULTY
    {
        EASY = 0, MEDIUM, HARD, DIABOLICAL, CHAOS, NOT_APPLYABLE,
    }

/*
 *  The Cube consists of a dungeon of Chambers (Rooms)
 *  Every Chamber has a type, position in The Cube, difficulty, number of rooms you need to pass to get to a Threshold/Threshold_Drop
 */
    private class Chamber
    {
        private int type;
        private ROOM_DIFFICULTY roomDifficulty;
        private float difficulty;
        private int level;
        private int x, y;
        private int closestThreshold;

        public Chamber()
        {
            this.type |= ROOM;
        }

        public Chamber(int level)
        {
            this.type |= ROOM;
            this.level = level;
        }

        public int getType() { return type; }
        public float getDifficulty() { return difficulty; }
        public ROOM_DIFFICULTY getRoomDifficulty() { return roomDifficulty; }

        public void setPosX(int x)
        {
            this.x = x;
        }

        public void setPosY(int y)
        {
            this.y = y;
        }

        public int getPosX()
        {
            return x;
        }

        public int getPosY()
        {
            return y;
        }

        public void setDifficulty(float difficulty)
        {
            this.difficulty = difficulty;
        }

        public void setRoomDifficulty(ROOM_DIFFICULTY roomDifficulty)
        {
            this.roomDifficulty = roomDifficulty;
        }

        public void addType(int t)
        {
            type |= t;
        }

        public void setClosestThreshold(int ct)
        {
            closestThreshold = ct;
        }

        public bool removeType(int t)
        {
            if ((type & t) == t)
            {
                type &= ~t;
                return true;
            } else return false;
        }

        public void calculateDifficulty(int side)
        {
            if (roomDifficulty != ROOM_DIFFICULTY.NOT_APPLYABLE)
            {
                float temp_difficulty = side - this.level;
                temp_difficulty *= (100f / side);
                temp_difficulty *= 0.25f;
                temp_difficulty *= Mathf.Pow(UnityEngine.Random.Range(0.9f, 1.0f), 2.0f);

                int longestPossible = ((2 * side) - 3);

                temp_difficulty +=
                    ((longestPossible - closestThreshold) * (100.0f / longestPossible) * 0.25f)
                    * Mathf.Pow(UnityEngine.Random.Range(0.7f, 1.0f), 2.0f);


                temp_difficulty += 50.0f * Mathf.Pow(UnityEngine.Random.Range(0.0f, 1.0f), 3.0f);

                difficulty = temp_difficulty;

                if (difficulty <= 100 && difficulty >= 70) roomDifficulty = ROOM_DIFFICULTY.DIABOLICAL;
                else if (difficulty <= 70 && difficulty >= 45) roomDifficulty = ROOM_DIFFICULTY.HARD;
                else if (difficulty <= 45 && difficulty >= 40) roomDifficulty = ROOM_DIFFICULTY.CHAOS;
                else if (difficulty <= 40 && difficulty >= 25) roomDifficulty = ROOM_DIFFICULTY.MEDIUM;
                else if (difficulty <= 25 && difficulty >= 0) roomDifficulty = ROOM_DIFFICULTY.EASY;
            }
        }
        
        //type
        //roomDifficulty
        //difficulty
        //x, y
        //closestThreshold

        public void debugPrint()
        {
            Debug.Log($"type {type}: roomDifficulty={roomDifficulty}, difficulty%={difficulty}, (x, y)={x}, {y}, closestThreshold={closestThreshold}");
        }
    }

    private class Level
    {
        private int level;
        private int size;
        private int thresholdIndex;
        private int thresholdDropIndex;
        private int numberOfRooms;
        private Chamber[] Chambers;

        public Level(int level, int numberOfRooms, int size)
        {
            this.level = level;
            this.numberOfRooms = numberOfRooms;
            this.size = size;
            this.Chambers = new Chamber[numberOfRooms];

            for (int i = 0; i < numberOfRooms; i++) {
                Chambers[i] = new Chamber(level);

                Chambers[i].setPosX(i % size);
                Chambers[i].setPosY(i / size);
            }
        }

        public int getDrop()        { return thresholdDropIndex; }
        public int getThreshold()   { return thresholdIndex; }
        public int getLevel()       { return level; }

        public void setRoomDifficulty(int index, float difficulty, ROOM_DIFFICULTY roomDifficulty)
        {
            Chambers[index].setDifficulty(difficulty);
            Chambers[index].setRoomDifficulty(roomDifficulty);
        }

        public void addChamberType(int index, int type) 
        {
            Chambers[index].addType(type);
        }

        public void removeChamberType(int index, int type)
        {
            Chambers[index].removeType(type);
        }
        public void collectRooms(List<RoomInfo> list)
        {
            foreach (Chamber c in Chambers)
                list.Add(new RoomInfo
                {
                    level = this.level,
                    x = c.getPosX(),
                    y = c.getPosY(),
                    type = c.getType(),
                    difficulty = c.getDifficulty(),
                    difficultyCategory = (int)c.getRoomDifficulty()
                });
        }

        public int generateThreshold(int thresholdDrop)
        {
            this.thresholdDropIndex = thresholdDrop;

            bool sorted = false;

            int random = 0;

            List<int> neighbours = getNeighbours(thresholdDropIndex);

            while (!sorted)
            {
                random = UnityEngine.Random.Range(0, numberOfRooms);

                if (neighbours.Contains(random)) sorted = false;
                else if (random == thresholdDrop) sorted = false;
                else sorted = true;
            }

            this.thresholdIndex = random;

            return this.thresholdIndex;
        }

        public int distanceToThreshold(int index)
        {
            int x = index % size;
            int y = index / size;
            int tx = thresholdIndex % size;
            int ty = thresholdIndex / size;

            return Math.Max(Math.Abs(x - tx), Math.Abs(y - ty));
        }

        public int distanceToThreshold(int x, int y)
        {
            int tx = thresholdIndex % size;
            int ty = thresholdIndex / size;

            return Math.Max(Math.Abs(x - tx), Math.Abs(y - ty));
        }

        public void calculateDifficulty()
        {
            foreach (Chamber chamber in Chambers)
                chamber.setClosestThreshold(this.distanceToThreshold(chamber.getPosX(), chamber.getPosY()));

            foreach (Chamber chamber in Chambers)
                chamber.calculateDifficulty(size);
        }

        public List<int> getNeighbours(int index)
        {
            List<int> neighbours = new List<int>();

            int row = index / size;
            int col = index % size;

            for (int dr = -1; dr <= 1; dr++)
            {
                for (int dc = -1; dc <= 1; dc++)
                {
                    if (dr == 0 && dc == 0) continue; // skipping same cell

                    int newRow = row + dr;
                    int newCol = col + dc;

                    // grid border — blocks goin out of bound
                    if (newRow >= 0 && newRow < size && newCol >= 0 && newCol < size)
                    {
                        int neighbourIndex = newRow * size + newCol;

                        // safety check if its not a square
                        if (neighbourIndex < numberOfRooms && neighbourIndex >= 0)
                            neighbours.Add(neighbourIndex);
                    }
                }
            }

            return neighbours;
        }

        public void debugPrint()
        {
            Debug.Log("< Level >");
            Debug.Log($"Poziom {level}: drop={thresholdDropIndex}, threshold={thresholdIndex}");

            foreach (Chamber chamber in Chambers)
            {
                chamber.debugPrint();
            }
        }
    }
}
