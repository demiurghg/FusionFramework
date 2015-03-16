

class Player {
	Player (
}

class Tank : Player {
}

class Monster : Player {
}

class Ammo {
}

class HealthPack : Ammo{
}

class RocketAmmo : Ammo {
}

class LaserAmmo : Ammo {
}

class Rocket {
}


string level = "tank 12 10; rocketAmmo 10 15";

vector<Player*> players;
struct CreatorItem {
	string name;
	void *func;
}

vector<CreatorItem> items; 

void loadLevel()
{
	for ( each line ) {
		line.name
		line.x
		line.y
		
		Player *p;
		
		for ( each item ) {
			if (item.name == line.name) {
				p = item.func();
			}
		}
		
		// if (line.name=="Tank") {
			// p = new Tank();
		// }
		// if (line.name=="Monster") {
			// p = new Monster();
		// }
		
		players.push_back( p );
	}
}

CreateTank () { return new Tank(); }

void main()
{
	playerCreator.Register( "Tank", CreateTank );
	playerCreator.Register( "Monster", CreateMonster );
	
	loadLevel()
	
	while (1) {
		
		
		
		
	}
}