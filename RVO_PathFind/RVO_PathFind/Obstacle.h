#ifndef RVO_OBSTACLE_H_
#define RVO_OBSTACLE_H_

#include "Definitions.h"

namespace RVO {

	class Obstacle {
	private:

		Obstacle();

		bool isConvex_;
		Obstacle *nextObstacle_;
		Vector2 point_;
		Obstacle *prevObstacle_;
		Vector2 unitDir_;

		size_t id_;

		friend class Agent;
		friend class KdTree;
		friend class RVOSimulator;
	};
}

#endif /* RVO_OBSTACLE_H_ */
