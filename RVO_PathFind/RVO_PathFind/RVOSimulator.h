#ifndef RVO_RVO_SIMULATOR_H_
#define RVO_RVO_SIMULATOR_H_

#include <cstddef>
#include <limits>
#include <vector>
#include <stack>

#include "Vector2.h"

namespace RVO {
	const size_t RVO_ERROR = 10000000;

	class Line {
	public:

		Vector2 point;

		Vector2 direction;
	};

	enum AgentType
	{
		Ground = 0,
		Fly,
	};

    enum MonsterType
    {
        NormalGround = 0,
        NormalFly,
        SpecialGround,
		IgnoreAll,
        Hero = 4,
    };

	class Agent;
	class KdTree;
	class Obstacle;

	class RVOSimulator {
	public:
		RVOSimulator();

		RVOSimulator(Fix16 timeStep, Fix16 neighborDist, size_t maxNeighbors,
					 Fix16 timeHorizon, Fix16 timeHorizonObst, Fix16 radius,
					 Fix16 maxSpeed, const Vector2 &velocity = Vector2());

		~RVOSimulator();

		size_t addAgent(const Vector2 &position);

		size_t addAgent(const Vector2 &position, Fix16 neighborDist,
						size_t maxNeighbors, Fix16 timeHorizon,
						Fix16 timeHorizonObst, Fix16 radius, Fix16 maxSpeed, Fix16 mass,
                        MonsterType monsterType,
						const Vector2 &velocity = Vector2());

		size_t addObstacle(const std::vector<Vector2> &vertices);
        void ClearObstacle();

		void doStep();

		void doStepBuildTree();
		void doStepNeighborAndVelocity(int index, int max);
		void doStepUpdate();
		void doStepAddDeltaTime();

		size_t getAgentAgentNeighbor(size_t agentNo, size_t neighborNo) const;

		size_t getAgentMaxNeighbors(size_t agentNo) const;

		Fix16 getAgentMaxSpeed(size_t agentNo) const;

		Fix16 getAgentNeighborDist(size_t agentNo) const;

		size_t getAgentNumAgentNeighbors(size_t agentNo) const;

		size_t getAgentNumObstacleNeighbors(size_t agentNo) const;

		size_t getAgentNumORCALines(size_t agentNo) const;

		size_t getAgentObstacleNeighbor(size_t agentNo, size_t neighborNo) const;

		const Line &getAgentORCALine(size_t agentNo, size_t lineNo) const;

		const Vector2 &getAgentPosition(size_t agentNo) const;

		const Vector2 &getAgentPrefVelocity(size_t agentNo) const;

		Fix16 getAgentRadius(size_t agentNo) const;

		Fix16 getAgentTimeHorizon(size_t agentNo) const;

		Fix16 getAgentTimeHorizonObst(size_t agentNo) const;

		const Vector2 &getAgentVelocity(size_t agentNo) const;

		Fix16 getGlobalTime() const;

		size_t getNumAgents() const;

		size_t getNumObstacleVertices() const;

		const Vector2 &getObstacleVertex(size_t vertexNo) const;

		size_t getNextObstacleVertexNo(size_t vertexNo) const;

		size_t getPrevObstacleVertexNo(size_t vertexNo) const;

		Fix16 getTimeStep() const;

		void processObstacles();

		bool queryVisibility(const Vector2 &point1, const Vector2 &point2,
							 Fix16 radius = Fix16::zero) const;

		void setAgentDefaults(Fix16 neighborDist, size_t maxNeighbors,
							  Fix16 timeHorizon, Fix16 timeHorizonObst,
							  Fix16 radius, Fix16 maxSpeed,
							  const Vector2 &velocity = Vector2());

		void setAgentMaxNeighbors(size_t agentNo, size_t maxNeighbors);

		void setAgentMaxSpeed(size_t agentNo, Fix16 maxSpeed);

		void setAgentNeighborDist(size_t agentNo, Fix16 neighborDist);

		void setAgentPosition(size_t agentNo, const Vector2 &position);

		void setAgentPrefVelocity(size_t agentNo, const Vector2 &prefVelocity);

		void setAgentRadius(size_t agentNo, Fix16 radius);

		void setAgentTimeHorizon(size_t agentNo, Fix16 timeHorizon);

		void setAgentTimeHorizonObst(size_t agentNo, Fix16 timeHorizonObst);

		void setAgentVelocity(size_t agentNo, const Vector2 &velocity);

		void setTimeStep(Fix16 timeStep);

		void removeAgent(size_t agentIndex);
		void setAgentMass(size_t agentIndex, Fix16 mass);
		int QueryNearByAgents(Vector2 pos, int *ptr, int currentCount, int size, Fix16 rangeSq, int addId);

	private:
		std::vector<Agent *> agents_;
		Agent *defaultAgent_;
		Fix16 globalTime_;
		KdTree *kdTree_;
		std::vector<Obstacle *> obstacles_;
		Fix16 timeStep_;

		std::stack<size_t> m_UnusedAgentIndexs;

		friend class Agent;
		friend class KdTree;
		friend class Obstacle;
	};
}

#endif
