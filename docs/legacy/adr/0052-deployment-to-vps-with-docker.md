# ADR 0052: Deployment to VPS with Docker

## Status
Accepted

## Date
2025-07-28

## Context
For deploying the Kedr E-Commerce Platform, we need a reliable, cost-effective, and scalable hosting solution. The application consists of multiple services that need to be deployed, maintained, and scaled independently. We considered several deployment options:

- Cloud Platform as a Service (PaaS) solutions like Azure App Service or AWS Elastic Beanstalk
- Managed Kubernetes services (AKS, EKS, GKE)
- Traditional VPS hosting with manual deployments
- VPS hosting with containerization

Each option has different trade-offs regarding cost, complexity, scalability, and operational overhead.

## Decision
We will deploy the Kedr E-Commerce Platform on a Virtual Private Server (VPS) using Docker containers orchestrated with Docker Compose for the following reasons:

1. **Cost-effectiveness**: VPS providers offer predictable pricing models that are generally more economical for our current scale than managed cloud services.

2. **Containerization benefits**: Using Docker provides:
   - Consistent environments across development and production
   - Isolation between services
   - Simplified dependency management
   - Easy horizontal scaling by adding container instances

3. **Infrastructure as Code**: Docker Compose allows us to define our entire infrastructure stack in code, making it reproducible and version-controlled.

4. **Simplified operations**: Docker's standardized container format makes deployment, updates, and rollbacks more straightforward than traditional deployments.

5. **Resource efficiency**: Containers share the host OS kernel, making them more lightweight than VMs while still providing good isolation.

## Consequences

### Positive
1. Simplified deployment process with standardized container images
2. Consistent environments between development, testing, and production
3. Easier scaling by adding more container instances or VPS nodes
4. Better resource utilization compared to traditional deployment
5. Lower costs compared to managed cloud services for our current scale
6. Simplified dependency management with containerized services

### Negative
1. Requires DevOps knowledge for maintaining Docker infrastructure
2. Manual scaling compared to managed Kubernetes solutions
3. Self-managed infrastructure requires more operational oversight
4. Potential for higher operational complexity as the application grows
5. Network configuration between containers requires careful planning

## Implementation

The deployment will be structured as follows:

1. **Base VPS Configuration**:
   - Ubuntu Server LTS as the host operating system
   - Docker Engine and Docker Compose installed
   - Nginx as a reverse proxy for routing traffic
   - Let's Encrypt for SSL/TLS certificates

2. **Docker Compose Stack**:
```yaml
version: '3.8'

services:
  web:
    image: kedrstore/web:latest
    restart: always
    depends_on:
      - api
    environment:
      - API_URL=http://api:5000

  api:
    image: kedrstore/api:latest
    restart: always
    depends_on:
      - postgres
      - redis
    environment:
      - ConnectionStrings__DefaultConnection=Host=postgres;Database=kedrstore;Username=kedr;Password=password
      - Redis__ConnectionString=redis:6379

  postgres:
    image: postgres:15
    volumes:
      - postgres-data:/var/lib/postgresql/data
    environment:
      - POSTGRES_USER=kedr
      - POSTGRES_PASSWORD=password
      - POSTGRES_DB=kedrstore

  redis:
    image: redis:7
    volumes:
      - redis-data:/data

volumes:
  postgres-data:
  redis-data:
```

3. **CI/CD Pipeline**:
   - Automated builds of Docker images on code changes
   - Image versioning and tagging
   - Automated testing before deployment
   - Secure transfer of images to production
   - Controlled deployment with rollback capability
