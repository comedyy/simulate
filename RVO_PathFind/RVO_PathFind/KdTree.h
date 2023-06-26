#ifndef RVO_KD_TREE_H_
#define RVO_KD_TREE_H_

#include "Definitions.h"

namespace RVO {

	class KdTree {
	private:

		class AgentTreeNode {
		public:

			size_t begin;

			size_t end;

			size_t left;

			float maxX;

			float maxY;

			float minX;

			float minY;

			size_t right;
		};

		class ObstacleTreeNode {
		public:

			ObstacleTreeNode *left;

			const Obstacle *obstacle;

			ObstacleTreeNode *right;
		};

		explicit KdTree(RVOSimulator *sim);

		~KdTree();

		void buildAgentTree();

		void buildAgentTreeRecursive(size_t begin, size_t end, size_t node);

		void buildObstacleTree();

		ObstacleTreeNode *buildObstacleTreeRecursive(const std::vector<Obstacle *> &
													 obstacles);

		void computeAgentNeighbors(Agent *agent, float &rangeSq) const;

		void computeObstacleNeighbors(Agent *agent, float rangeSq) const;

		void deleteObstacleTree(ObstacleTreeNode *node);

		void queryAgentTreeRecursive(Agent *agent, float &rangeSq,
									 size_t node) const;

		void queryObstacleTreeRecursive(Agent *agent, float rangeSq,
										const ObstacleTreeNode *node) const;

		bool queryVisibility(const Vector2 &q1, const Vector2 &q2,
							 float radius) const;

		bool queryVisibilityRecursive(const Vector2 &q1, const Vector2 &q2,
									  float radius,
									  const ObstacleTreeNode *node) const;

		int QueryNearByAgents(const Vector2 &pos, int* ptr, int addCount, int totalCount, float range, int addId) const;

		void QueryNearByAgents(const Vector2 &pos, int* ptr, int& addCount, int totalCount, float rangeSq, size_t node, int addId) const;

		std::vector<Agent *> agents_;
		std::vector<AgentTreeNode> agentTree_;
		ObstacleTreeNode *obstacleTree_;
		RVOSimulator *sim_;

		static const size_t MAX_LEAF_SIZE = 10;

		friend class Agent;
		friend class RVOSimulator;
	};
}

#endif /* RVO_KD_TREE_H_ */
