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

		void insertAgentNeighbor(const Agent *agent, float &rangeSq);

		void insertObstacleNeighbor(const Obstacle *obstacle, float rangeSq);

		void update();
        
        void SetMonsterType(MonsterType monsterType);
        
        MonsterType GetMonsterType();

		std::vector<std::pair<float, const Agent *> > agentNeighbors_;
		size_t maxNeighbors_;
		float maxSpeed_;
		float neighborDist_;
		Vector2 newVelocity_;
		std::vector<std::pair<float, const Obstacle *> > obstacleNeighbors_;
		std::vector<Line> orcaLines_;
		Vector2 position_;
		Vector2 prefVelocity_;
		float radius_;
		RVOSimulator *sim_;
		float timeHorizon_;
		float timeHorizonObst_;
		Vector2 velocity_;

		size_t id_;
        
        MonsterType m_MonsterType;

		float mass_;
		bool m_IsRemove;

		friend class KdTree;
		friend class RVOSimulator;
	};

	bool linearProgram1(const std::vector<Line> &lines, size_t lineNo,
						float radius, const Vector2 &optVelocity,
						bool directionOpt, Vector2 &result);

	size_t linearProgram2(const std::vector<Line> &lines, float radius,
						  const Vector2 &optVelocity, bool directionOpt,
						  Vector2 &result);

	void linearProgram3(const std::vector<Line> &lines, size_t numObstLines, size_t beginLine,
						float radius, Vector2 &result);
}

#endif /* RVO_AGENT_H_ */
