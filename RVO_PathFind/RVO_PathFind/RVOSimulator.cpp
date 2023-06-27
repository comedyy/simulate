#include "RVOSimulator.h"

#include "Agent.h"
#include "KdTree.h"
#include "Obstacle.h"
#include "../libfixmath/fix16.hpp"

#ifdef _OPENMP
#include <omp.h>
#endif

namespace RVO {
	RVOSimulator::RVOSimulator() : defaultAgent_(NULL), globalTime_(Fix16::zero), kdTree_(NULL), timeStep_(Fix16::zero)
	{
		kdTree_ = new KdTree(this);
	}

	RVOSimulator::RVOSimulator(Fix16 timeStep, Fix16 neighborDist, size_t maxNeighbors, Fix16 timeHorizon,
		Fix16 timeHorizonObst, Fix16 radius, Fix16 maxSpeed, const Vector2 &velocity)
		: defaultAgent_(NULL), globalTime_(Fix16::zero), kdTree_(NULL), timeStep_(timeStep)
	{
		kdTree_ = new KdTree(this);
		defaultAgent_ = new Agent(this);

		defaultAgent_->maxNeighbors_ = maxNeighbors;
		defaultAgent_->maxSpeed_ = maxSpeed;
		defaultAgent_->neighborDist_ = neighborDist;
		defaultAgent_->radius_ = radius;
		defaultAgent_->timeHorizon_ = timeHorizon;
		defaultAgent_->timeHorizonObst_ = timeHorizonObst;
		defaultAgent_->velocity_ = velocity;
	}

	RVOSimulator::~RVOSimulator()
	{
		if (defaultAgent_ != NULL) {
			delete defaultAgent_;
		}

		for (size_t i = 0; i < agents_.size(); ++i) {
			delete agents_[i];
		}

        ClearObstacle();
        
		delete kdTree_;
	}

    void RVOSimulator::ClearObstacle()
    {
        for (size_t i = 0; i < obstacles_.size(); ++i) {
            delete obstacles_[i];
        }
        obstacles_.clear();
    }

	size_t RVOSimulator::addAgent(const Vector2 &position)
	{
		if (defaultAgent_ == NULL) {
			return RVO_ERROR;
		}

		Agent *agent = new Agent(this);

		agent->position_ = position;
		agent->maxNeighbors_ = defaultAgent_->maxNeighbors_;
		agent->maxSpeed_ = defaultAgent_->maxSpeed_;
		agent->neighborDist_ = defaultAgent_->neighborDist_;
		agent->radius_ = defaultAgent_->radius_;
		agent->timeHorizon_ = defaultAgent_->timeHorizon_;
		agent->timeHorizonObst_ = defaultAgent_->timeHorizonObst_;
		agent->velocity_ = defaultAgent_->velocity_;

		agent->id_ = agents_.size();

		agents_.push_back(agent);

		return agents_.size() - 1;
	}

	size_t RVOSimulator::addAgent(const Vector2 &position, Fix16 neighborDist, size_t maxNeighbors, Fix16 timeHorizon, Fix16 timeHorizonObst, 
		Fix16 radius, Fix16 maxSpeed, Fix16 mass, MonsterType monsterType,
        const Vector2 &velocity)
	{
		Agent* agent;
		size_t agentIndex;
		if (m_UnusedAgentIndexs.size() != 0)
		{
			agentIndex = m_UnusedAgentIndexs.top();
			m_UnusedAgentIndexs.pop();
			agent = agents_[agentIndex];
			agent->m_IsRemove = false;
		}
		else
		{
			agent = new Agent(this);
			agentIndex = agents_.size();
			agents_.push_back(agent);
		}


		//Agent *agent = new Agent(this);

		agent->position_ = position;
		agent->maxNeighbors_ = maxNeighbors;
		agent->maxSpeed_ = maxSpeed;
		agent->neighborDist_ = neighborDist;
		agent->radius_ = radius;
		agent->timeHorizon_ = timeHorizon;
		agent->timeHorizonObst_ = timeHorizonObst;
		agent->velocity_ = velocity;
		agent->mass_ = mass;

		//agent->id_ = agents_.size();
		agent->id_ = agentIndex;
        
        agent->m_MonsterType = monsterType;

		//agents_.push_back(agent);

		return agentIndex;
	}

	void RVOSimulator::removeAgent(size_t agentIndex)
	{
		Agent* agent = agents_[agentIndex];
		agent->position_ = Vector2(Fix16::zero, Fix16::zero);
		agent->velocity_ = Vector2(Fix16::zero, Fix16::zero);
		agent->maxSpeed_ = 0;
		agent->m_IsRemove = true;
		m_UnusedAgentIndexs.push(agentIndex);
	}

	size_t RVOSimulator::addObstacle(const std::vector<Vector2> &vertices)
	{
		if (vertices.size() < 2) {
			return RVO_ERROR;
		}

		const size_t obstacleNo = obstacles_.size();

		for (size_t i = 0; i < vertices.size(); ++i) {
			Obstacle *obstacle = new Obstacle();
			obstacle->point_ = vertices[i];

			if (i != 0) {
				obstacle->prevObstacle_ = obstacles_.back();
				obstacle->prevObstacle_->nextObstacle_ = obstacle;
			}

			if (i == vertices.size() - 1) {
				obstacle->nextObstacle_ = obstacles_[obstacleNo];
				obstacle->nextObstacle_->prevObstacle_ = obstacle;
			}

			obstacle->unitDir_ = normalize(vertices[(i == vertices.size() - 1 ? 0 : i + 1)] - vertices[i]);

			if (vertices.size() == 2) {
				obstacle->isConvex_ = true;
			}
			else {
				obstacle->isConvex_ = (leftOf(vertices[(i == 0 ? vertices.size() - 1 : i - 1)], vertices[i], vertices[(i == vertices.size() - 1 ? 0 : i + 1)]) >= Fix16::zero);
			}

			obstacle->id_ = obstacles_.size();

			obstacles_.push_back(obstacle);
		}

		return obstacleNo;
	}

	void RVOSimulator::doStep()
	{
		kdTree_->buildAgentTree();

#ifdef _OPENMP
#pragma omp parallel for num_threads(2)
#endif
		for (int i = 0; i < static_cast<int>(agents_.size()); ++i) {
			if (agents_[i]->m_IsRemove)
				continue;
			agents_[i]->computeNeighbors();
			agents_[i]->computeNewVelocity();
		}

#ifdef _OPENMP
#pragma omp parallel for  num_threads(2)
#endif
		for (int i = 0; i < static_cast<int>(agents_.size()); ++i) {
			if (agents_[i]->m_IsRemove)
				continue;

			agents_[i]->update();
		}

		globalTime_ += timeStep_;
	}

	size_t RVOSimulator::getAgentAgentNeighbor(size_t agentNo, size_t neighborNo) const
	{
		return agents_[agentNo]->agentNeighbors_[neighborNo].second->id_;
	}

	size_t RVOSimulator::getAgentMaxNeighbors(size_t agentNo) const
	{
		return agents_[agentNo]->maxNeighbors_;
	}

	Fix16 RVOSimulator::getAgentMaxSpeed(size_t agentNo) const
	{
		return agents_[agentNo]->maxSpeed_;
	}

	Fix16 RVOSimulator::getAgentNeighborDist(size_t agentNo) const
	{
		return agents_[agentNo]->neighborDist_;
	}

	size_t RVOSimulator::getAgentNumAgentNeighbors(size_t agentNo) const
	{
		return agents_[agentNo]->agentNeighbors_.size();
	}

	size_t RVOSimulator::getAgentNumObstacleNeighbors(size_t agentNo) const
	{
		return agents_[agentNo]->obstacleNeighbors_.size();
	}

	size_t RVOSimulator::getAgentNumORCALines(size_t agentNo) const
	{
		return agents_[agentNo]->orcaLines_.size();
	}

	size_t RVOSimulator::getAgentObstacleNeighbor(size_t agentNo, size_t neighborNo) const
	{
		return agents_[agentNo]->obstacleNeighbors_[neighborNo].second->id_;
	}

	const Line &RVOSimulator::getAgentORCALine(size_t agentNo, size_t lineNo) const
	{
		return agents_[agentNo]->orcaLines_[lineNo];
	}

	const Vector2 &RVOSimulator::getAgentPosition(size_t agentNo) const
	{
		return agents_[agentNo]->position_;
	}

	const Vector2 &RVOSimulator::getAgentPrefVelocity(size_t agentNo) const
	{
		return agents_[agentNo]->prefVelocity_;
	}

	Fix16 RVOSimulator::getAgentRadius(size_t agentNo) const
	{
		return agents_[agentNo]->radius_;
	}

	Fix16 RVOSimulator::getAgentTimeHorizon(size_t agentNo) const
	{
		return agents_[agentNo]->timeHorizon_;
	}

	Fix16 RVOSimulator::getAgentTimeHorizonObst(size_t agentNo) const
	{
		return agents_[agentNo]->timeHorizonObst_;
	}

	const Vector2 &RVOSimulator::getAgentVelocity(size_t agentNo) const
	{
		return agents_[agentNo]->velocity_;
	}

	Fix16 RVOSimulator::getGlobalTime() const
	{
		return globalTime_;
	}

	// ���� - δʹ������
	size_t RVOSimulator::getNumAgents() const
	{
		return agents_.size() - m_UnusedAgentIndexs.size();
	}

	size_t RVOSimulator::getNumObstacleVertices() const
	{
		return obstacles_.size();
	}

	const Vector2 &RVOSimulator::getObstacleVertex(size_t vertexNo) const
	{
		return obstacles_[vertexNo]->point_;
	}

	size_t RVOSimulator::getNextObstacleVertexNo(size_t vertexNo) const
	{
		return obstacles_[vertexNo]->nextObstacle_->id_;
	}

	size_t RVOSimulator::getPrevObstacleVertexNo(size_t vertexNo) const
	{
		return obstacles_[vertexNo]->prevObstacle_->id_;
	}

	Fix16 RVOSimulator::getTimeStep() const
	{
		return timeStep_;
	}

	void RVOSimulator::processObstacles()
	{
		kdTree_->buildObstacleTree();
	}

	bool RVOSimulator::queryVisibility(const Vector2 &point1, const Vector2 &point2, Fix16 radius) const
	{
		return kdTree_->queryVisibility(point1, point2, radius);
	}

	void RVOSimulator::setAgentDefaults(Fix16 neighborDist, size_t maxNeighbors, Fix16 timeHorizon, Fix16 timeHorizonObst, Fix16 radius, Fix16 maxSpeed, const Vector2 &velocity)
	{
		if (defaultAgent_ == NULL) {
			defaultAgent_ = new Agent(this);
		}

		defaultAgent_->maxNeighbors_ = maxNeighbors;
		defaultAgent_->maxSpeed_ = maxSpeed;
		defaultAgent_->neighborDist_ = neighborDist;
		defaultAgent_->radius_ = radius;
		defaultAgent_->timeHorizon_ = timeHorizon;
		defaultAgent_->timeHorizonObst_ = timeHorizonObst;
		defaultAgent_->velocity_ = velocity;
	}

	void RVOSimulator::setAgentMaxNeighbors(size_t agentNo, size_t maxNeighbors)
	{
		agents_[agentNo]->maxNeighbors_ = maxNeighbors;
	}

	void RVOSimulator::setAgentMaxSpeed(size_t agentNo, Fix16 maxSpeed)
	{
		agents_[agentNo]->maxSpeed_ = maxSpeed;
	}

	void RVOSimulator::setAgentNeighborDist(size_t agentNo, Fix16 neighborDist)
	{
		agents_[agentNo]->neighborDist_ = neighborDist;
	}

	void RVOSimulator::setAgentPosition(size_t agentNo, const Vector2 &position)
	{
		agents_[agentNo]->position_ = position;
	}

	void RVOSimulator::setAgentPrefVelocity(size_t agentNo, const Vector2 &prefVelocity)
	{
		agents_[agentNo]->prefVelocity_ = prefVelocity;
	}

	void RVOSimulator::setAgentRadius(size_t agentNo, Fix16 radius)
	{
		agents_[agentNo]->radius_ = radius;
	}

	void RVOSimulator::setAgentTimeHorizon(size_t agentNo, Fix16 timeHorizon)
	{
		agents_[agentNo]->timeHorizon_ = timeHorizon;
	}

	void RVOSimulator::setAgentTimeHorizonObst(size_t agentNo, Fix16 timeHorizonObst)
	{
		agents_[agentNo]->timeHorizonObst_ = timeHorizonObst;
	}

	void RVOSimulator::setAgentVelocity(size_t agentNo, const Vector2 &velocity)
	{
		agents_[agentNo]->velocity_ = velocity;
	}

	void RVOSimulator::setTimeStep(Fix16 timeStep)
	{
		timeStep_ = timeStep;
	}

	void RVOSimulator::setAgentMass(size_t agentIndex, Fix16 mass)
	{
		agents_[agentIndex]->mass_ = mass;
	}

	int RVOSimulator::QueryNearByAgents(Vector2 pos, int *ptr, int currentCount, int size, Fix16 range, int addId)
	{
		return kdTree_->QueryNearByAgents(pos, ptr, currentCount, size, range, addId);
	}

	// test performance
	void RVOSimulator::doStepBuildTree()
	{
		kdTree_->buildAgentTree();
	}

	void RVOSimulator::doStepNeighborAndVelocity(int index, int max)
	{
		int totalSize = static_cast<int>(agents_.size());
		float fromPercent = index * 1.0f / max;
		float toPercent = (index + 1) * 1.0f / max;
		if (index == max - 1) 
		{
			toPercent = 1;
		}

		int fromIndex = (int)floor(fromPercent * totalSize);
		int ToIndex = (int)floor(toPercent * totalSize);

		for (int i = fromIndex; i < ToIndex; ++i) {
			if (agents_[i]->m_IsRemove)
				continue;
			agents_[i]->computeNeighbors();
			agents_[i]->computeNewVelocity();
		}
	}

	void RVOSimulator::doStepUpdate()
	{
		for (int i = 0; i < static_cast<int>(agents_.size()); ++i) {
			if (agents_[i]->m_IsRemove)
				continue;

			agents_[i]->update();
		}

		globalTime_ += timeStep_;
	}
}
