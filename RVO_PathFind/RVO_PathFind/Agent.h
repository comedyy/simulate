#ifndef RVO_AGENT_H_
#define RVO_AGENT_H_

#include "Definitions.h"
#include "RVOSimulator.h"

namespace RVO {

	class Agent {
	private:

		Agent(RVOSimulator *sim);

		void computeNeighbors();

		void computeNewVelocity();

		void insertAgentNeighbor(const Agent *agent, Fix16 &rangeSq);

		void insertObstacleNeighbor(const Obstacle *obstacle, Fix16 rangeSq);

		void update();
        
        void SetMonsterType(MonsterType monsterType);
        
        MonsterType GetMonsterType();

		std::vector<std::pair<Fix16, const Agent *> > agentNeighbors_;
		size_t maxNeighbors_;
		Fix16 maxSpeed_;
		Fix16 neighborDist_;
		Vector2 newVelocity_;
		std::vector<std::pair<Fix16, const Obstacle *> > obstacleNeighbors_;
		std::vector<Line> orcaLines_;
		Vector2 position_;
		Vector2 prefVelocity_;
		Fix16 radius_;
		RVOSimulator *sim_;
		Fix16 timeHorizon_;
		Fix16 timeHorizonObst_;
		Vector2 velocity_;

		size_t id_;
		int entityId;

        MonsterType m_MonsterType;

		Fix16 mass_;
		bool m_IsRemove;

		friend class KdTree;
		friend class RVOSimulator;
	};

	bool linearProgram1(const std::vector<Line> &lines, size_t lineNo,
						Fix16 radius, const Vector2 &optVelocity,
						bool directionOpt, Vector2 &result);

	size_t linearProgram2(const std::vector<Line> &lines, Fix16 radius,
						  const Vector2 &optVelocity, bool directionOpt,
						  Vector2 &result);

	void linearProgram3(const std::vector<Line> &lines, size_t numObstLines, size_t beginLine,
						Fix16 radius, Vector2 &result);
}

#endif /* RVO_AGENT_H_ */
