// Kyle Zapalac, 03-Apr-2022, GAME-1343 (SP 2022)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO; // for file reading/writing
using System.Text.RegularExpressions; // regex
using System.Threading; // for delays

namespace Wk10_TurnBasedRPG_KZ
{
    /* Main Program Class
     *  Create a LIST of fighters (the turn order) and create and add combatants
     *  Create a game loop to:
     *   1) Continously go thru the list of fighters and execute each turn by calling fighter's TakeAction
     *   2) Continue until one fighter remains and display a victory/loss message (I changed this to defeating enemies)
     */
    class Program
    {
        static void Main(string[] args)
        {
            int delay = 1000; // delay to be used to slow output in console

            // create players and add them to a list
            Player p1 = new Player("Kyle", 20, 25); // Kyle starts with 20hp, 25 mana
            Player p2 = new Player("Andrea", 30, 0); // Andrea starts with 30hp, 0 mana

            List<Player> players = new List<Player>();
            players.Add(p1);
            players.Add(p2);

            // create enemies and add them to a list
            Enemy e1 = new Enemy("Plague Rat", 15, 10); // Plague Rat starts with 15hp, 10 mana
            Enemy e2 = new Enemy("Zombie", 25, 10); // Zombie starts with 25hp, 10 mana

            List<Enemy> enemies = new List<Enemy>();
            enemies.Add(e1);
            enemies.Add(e2);

            // setup player allies and targets
            p1.setTargets(e1, e2);
            p1.setAlly(p2);
            p2.setTargets(e1, e2);
            p2.setAlly(p1);

            // setup enemy allies and targets
            e1.setTargets(p1, p2);
            e1.setAlly(e2);
            e2.setTargets(p1, p2);
            e2.setAlly(e1);

            // gameplay loop continues until enemies or players are dead
            Console.WriteLine("====================================================================\n");
            Console.WriteLine("GAME STARTING!\n");
            
            Thread.Sleep(delay); // adding delay for easier reading
            int roundCount = 1;
            while ((!p1.Dead || !p2.Dead) && (!e1.Dead || !e2.Dead))
            {
                // announce start of round
                Console.WriteLine("====================================================================\n");
                Console.WriteLine("STARTING ROUND " + roundCount + "!\n");
                roundCount++;

                // players' turn
                foreach (Player player in players)
                {
                    Console.WriteLine("================================================\n");

                    Thread.Sleep(delay); // adding delay to player turns for easier reading

                    if (!player.Dead)
                    {
                        Console.WriteLine(player.Name + " has " + player.CurrentHealth + "/" + player.MaxHealth + " hp and is ready for action!");
                        player.TakeAction();
                    }
                    else
                    {
                        Console.WriteLine(player.Name + " is DEAD!\n");
                    }
                }

                // enemies' turn
                foreach (Enemy enemy in enemies)
                {
                    Console.WriteLine("================================================\n");

                    Thread.Sleep(delay); // adding delay to enemy turns for easier reading

                    if (!enemy.Dead)
                    {
                        Console.WriteLine(enemy.Name + " has " + enemy.CurrentHealth + "/" + enemy.MaxHealth + " hp and is ready for action!");
                        enemy.TakeAction();
                    }
                    else
                    {
                        Console.WriteLine(enemy.Name + " is DEAD!\n");
                    }
                }
            }

            // victory/loss message
            Console.WriteLine("====================================================================\n");
            Console.WriteLine("GAME OVER!!!");
            if (p1.Dead && p2.Dead)
            {
                Console.WriteLine("\n" + p1.Name + " and " + p2.Name + " have LOST!!!");
            }
            else
            {
                Console.WriteLine("\n" + p1.Name + " and " + p2.Name + " have WON!!!");
            }
            Console.WriteLine("\n====================================================================\n");
        }
    }

    /* Fighter Class 
     *  With these Variables:
     *   1) Current and maximum health -DONE
     *   2) Whether the fighter is dead -DONE
     *   3) A reference to another Fighter, an opponent (I moved this to the other classes because it can either be player or enemy)
     *  With these Methods:
     *   1) A method for taking damage (decrease current health by a given amount, call death function if health reaches 0)
     *   2) A method for when the fighter dies (set its "dead" variable to true)
     *   3) A method for taking an action, which will be overridden by the Player and Enemy classes (the player will have a 
     *      choice of options, while the enemy will simply attack the player)
     */

    abstract class Fighter
    {
        string name;
        int currentHealth;
        int maxHealth;
        int currentMana;
        int maxMana;
        bool dead;

        public Fighter(string n, int mh, int mm)
        {
            name = n;
            currentHealth = mh; // health is full by default
            maxHealth = mh;
            currentMana = mm;// mana is full by default
            maxMana = mm;
            dead = false;
        }

        public string Name
        {
            get { return name; }    
            set { name = value; }
        }

        public int CurrentHealth
        {
            get { return currentHealth; } 
            set { currentHealth = value; }
        }
        public int MaxHealth
        {
            get { return maxHealth; }
            set { maxHealth = value; }
        }

        public int CurrentMana
        {
            get { return currentMana; }
            set { currentMana = value; }
        }

        public int MaxMana
        {
            get { return maxMana; }
            set { maxMana = value; }
        }

        public bool Dead
        {
            get { return dead; }
            set { dead = value; }
        }

        public void TakeDamage(int d)
        {
            // reduce current health by amount of damage
            currentHealth -= d;
        }

        public void Die()
        {
            // set dead to true
            dead = true;
        }

        public virtual void TakeAction()
        {
            // this method shouldn't be called
            Console.WriteLine("An action is taken! Or is it?");
        }
    }

    /* Enemy Class
     *  Inherit from fighter -DONE
     *  Override the TakeAction function to cause the player to take damage. -DONE
     *  Print to console displaying what the enemy did & how much damage was caused. -DONE
     */

    class Enemy : Fighter
    {
        // variables
        string name;
        int currentHealth;
        int maxHealth;
        int currentMana;
        int maxMana;
        bool dead = false; // enemy is alive by default
        Enemy ally;
        Player target1;
        Player target2;

        // constructor
        public Enemy(string n, int mh, int mm) : base(n, mh, mm)
        {
            name = n;
            currentHealth = mh; // health is full by default
            maxHealth = mh;
            currentMana = mm; // mana is full by default
            maxMana = mm;
        }

        // set ally as other enemy
        public void setAlly(Enemy e)
        {
            ally = e;
        }

        // set targets as players
        public void setTargets(Player p1, Player p2)
        {
            target1 = p1;
            target2 = p2;
        }

        // enemy performs a random action 
        public override void TakeAction()
        {
            // if ally is alive or mana is available, randomly select an action
            if(!ally.Dead && currentMana > 0)
            {
                // generate random action
                Random randomAction = new Random();
                int action = randomAction.Next(1,4);

                // execute the selected action (33% chance to be a heal)
                if (action == 1 || action == 2)
                {
                    AttackEnemy();
                }
                else if (action == 3)
                {
                    HealAlly();
                }
                else
                {
                    Console.WriteLine("Error!");
                }
            }
            // if enemies' ally is dead or enemy is out of mana, just attack a player
            else 
            {
                AttackEnemy();
            }
        }

        // enemy attacks a player
        public void AttackEnemy()
        {
            // generate random damage between 1-7
            Random randomDamage = new Random();
            int damage = randomDamage.Next(1,8);

            // randomly select a target
            Random randomTarget = new Random();
            int target = randomTarget.Next(1,3);

            // validate that enemy is not targeting a dead player
            if (target1.Dead)
            {
                target = 2;
            }
            else if (target2.Dead)
            {
                target = 1;
            }
            // execute attack against random target
            else if (target == 1)
            {
                // reduce target health
                target1.TakeDamage(damage);
                
                // print results
                if (target1.CurrentHealth <= 0)
                {
                    Console.WriteLine("\n" + name + " attacked " + target1.Name + " for " + damage + " damage!");
                    Console.WriteLine(target1.Name + " has been defeated!\n");
                    target1.Die();
                }
                else
                {
                    Console.WriteLine("\n" + name + " attacked " + target1.Name + " for " + damage + " damage, leaving them with " + target1.CurrentHealth + " health.\n");
                }
            }
            else if (target == 2)
            {
                // reduce target health
                target2.TakeDamage(damage);

                // print results
                if (target2.CurrentHealth <= 0)
                {
                    Console.WriteLine("\n" + name + " attacked " + target2.Name + " for " + damage + " damage!");
                    Console.WriteLine(target2.Name + " has been defeated!\n");
                    target2.Die();
                }
                else
                {
                    Console.WriteLine("\n" + name + " attacked " + target2.Name + " for " + damage + " damage, leaving them with " + target2.CurrentHealth + " health.\n");
                }
            }
            else
            {
                Console.WriteLine("Error!");
            }
        }

        // enemy heals their ally
        public void HealAlly()
        {
            // generate random health between 1-7
            Random randomHeal = new Random();
            int heal = randomHeal.Next(1,8) * -1; // multiply by -1 to take negative damage (healing)

            if (ally.currentHealth == ally.maxHealth)
            {
                // no change to health
                currentHealth = maxHealth;
                Console.WriteLine("\n" + name + " heals their ally!");
                Console.WriteLine(ally.Name + " was already at max health! What a waste!");
                Console.WriteLine(name + " has " + currentMana + " mana left.\n");
            }

            else
            {
                // perform healing
                ally.TakeDamage(heal);

                // validate no overhealing 
                if (ally.currentHealth > ally.maxHealth)
                {
                    ally.currentHealth = ally.maxHealth;
                    Console.WriteLine("\n" + name + " healed their ally to full health!");
                    Console.WriteLine(ally.Name + " now has " + ally.CurrentHealth + " health!");
                    Console.WriteLine(name + " has " + currentMana + " mana left.\n");
                }
                else
                {
                    Console.WriteLine("\n" + name + " healed their ally for " + (heal * -1) + " health! ");
                    Console.WriteLine(ally.Name + " now has " + ally.CurrentHealth + " health!");
                    Console.WriteLine(name + " has " + currentMana + " mana left.\n");
                }
            }
        }
    }

    /* Player Class
     *  Inherit from Fighter - DONE
     *  Include a TakeAction version of the function should allow for a basic menu with choices
     *   1) Attack Enemy (KZ split this into 2 options for target selection)
     *   2) Drink Potion (check inventory)
     *   3) Heal Ally (added by KZ)
     *  Options above should have their own functions
     */   

    class Player : Fighter
    {
        string name;
        int currentHealth;
        int maxHealth;
        int currentMana;
        int maxMana;
        bool dead = false; // player is alive by default
        int potionCount = 2; // start with 2 potions
        Player ally;
        Enemy target1;
        Enemy target2;

        public Player(string n, int mh, int mm) : base(n, mh, mm)
        {
            name = n;
            currentHealth = mh; // health is full by default
            maxHealth = mh;
            currentMana = mm;
            maxMana = mm;
        }

        public void setAlly(Player p)
        {
            ally = p;
        }

        public void setTargets(Enemy e1, Enemy e2)
        {
            target1 = e1;
            target2 = e2;
        }

        public override void TakeAction()
        {
            
            // prompt user for input
            Console.WriteLine("\nWhat action will " + name +" take?");
            Console.WriteLine(" 1) Attack Enemy [1-7 dmg]");
            Console.WriteLine(" 2) Drink 1 Health Potion [5 hp] (" + potionCount + "/2 potions left)");
            Console.WriteLine(" 3) Heal Ally for 5 mp [1-7 hp] (" + currentMana + "/" + maxMana + " mp left)");

            // read user input
            int choice = 0;
            while (choice == 0)
            {
                // get input
                Console.Write("\nEnter the number of " + name + "'s choice: ");
                choice = int.Parse(Console.ReadLine());

                // valid input is one of the menu options else prompt again
                switch (choice)
                {
                    case 1:
                        AttackEnemy();
                        break;
                    case 2:
                        if(potionCount == 0)
                        {
                            Console.WriteLine("\n" + name + " is out of potions! Try something else!");
                            choice = 0;
                        }
                        else
                        {
                            potionCount--;
                            DrinkPotion();
                        }
                        break;
                    case 3:
                        if (currentMana == 0)
                        {
                            Console.WriteLine("\n" + name + " is out of mana! Try something else!");
                            choice = 0;
                        }
                        else
                        {
                            currentMana -= 5;
                            HealAlly();
                        }
                        break;
                    default:
                        Console.WriteLine("Error! Enter 1 - 4 only!");
                        choice = 0;
                        break;
                }
            }
        }

        // attack an enemy
        public void AttackEnemy()
        {
            // generate random damage between 1-7
            Random randomDamage = new Random();
            int damage = randomDamage.Next(1,8);
            
            // prompt user to select target
            Console.WriteLine("\nWhich enemy does " + name + " want to attack?");
            Console.WriteLine(" 1) " + target1.Name);
            Console.WriteLine(" 2) " + target2.Name);

            // get input
            int target = 0;
            do
            {
                Console.Write("Enter target (1 or 2): ");
                target = int.Parse(Console.ReadLine());

                //validate input is 1 or 2
                if (target < 1 || target > 2)
                {
                    target = 0;
                }
            } while (target == 0);

            // execute attack and print output
            switch(target)
            {
                case 1:
                    target1.TakeDamage(damage);

                    if (target1.CurrentHealth <= 0)
                    {
                        Console.WriteLine("\n" + name + " attacked " + target1.Name + " for " + damage + " damage!");
                        Console.WriteLine(target1.Name + " has been defeated!\n");
                        target1.Die();
                    }
                    else
                    {
                        Console.WriteLine("\n" + name + " attacked " + target1.Name + " for " + damage + " damage, leaving them with " + target1.CurrentHealth + " health.\n");
                    }
                    break;
                case 2:
                    target2.TakeDamage(damage);

                    if (target2.CurrentHealth <= 0)
                    {
                        Console.WriteLine("\n" + name + " attacked " + target2.Name + " for " + damage + " damage!");
                        Console.WriteLine(target2.Name + " has been defeated!\n");
                        target2.Die();
                    }
                    else
                    {
                        Console.WriteLine("\n" + name + " attacked " + target2.Name + " for " + damage + " damage, leaving them with " + target2.CurrentHealth + " health.\n");
                    }
                    break;
                default:
                    break;
            }
        }

        // player heals themself
        public void DrinkPotion()
        { 
            this.CurrentHealth += 5;

            if(this.CurrentHealth > this.MaxHealth)
            {
                this.CurrentHealth = this.MaxHealth;
                Console.WriteLine("\n" + name + " drank a potion and returned to max health!");
                Console.WriteLine(name + " now has " + potionCount + " potions left.\n");
            }
            else
            {
                Console.WriteLine("\n" + name + " drank a potion and restored health!");
                Console.WriteLine(name + " now has " + this.CurrentHealth + " health!");
                Console.WriteLine(name + " now has " + potionCount + " potions left.\n");
            }
        }
        
        // player heals their ally
        public void HealAlly()
        {
            // generate random health between 1-7
            Random randomHeal = new Random();
            int heal = randomHeal.Next(1,8)*-1; // multiply by -1 to take negative damage (healing)

            if (ally.CurrentHealth == ally.MaxHealth)
            {
                // no change to health
                Console.WriteLine("\n" + name + " heals their ally!");
                Console.WriteLine(ally.Name + " was already at max health! What a waste!");
                Console.WriteLine(name + " has " + currentMana + " mana left.\n");
            }
            else
            {
                // perform healing
                ally.TakeDamage(heal);
                
                // validate no overhealing 
                if (ally.CurrentHealth > ally.MaxHealth)
                {
                    ally.CurrentHealth = ally.MaxHealth;
                    Console.WriteLine("\n" + name + " healed their ally to full health!");
                    Console.WriteLine(ally.Name + " now has " + ally.CurrentHealth + " health!");
                    Console.WriteLine(name + " has " + currentMana + " mana left.\n");
                }
                else
                {
                    Console.WriteLine("\n" + name + " healed their ally for " + (heal * -1) + " health! ");
                    Console.WriteLine(ally.Name + " now has " + ally.CurrentHealth + " health!");
                    Console.WriteLine(name + " has " + currentMana + " mana left.\n");
                }
            }
        }
    }
}

