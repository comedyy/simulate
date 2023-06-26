#include "Obstacle.h"
#include "RVOSimulator.h"

namespace RVO {
	Obstacle::Obstacle() : isConvex_(false), nextObstacle_(NULL), prevObstacle_(NULL), id_(0) { }
}
