pipeline {
    agent any

    environment {
        // Имя вашего будущего образа
        IMAGE_NAME = "bank-app-api"
        TAG = "${env.BUILD_NUMBER}"
    }

    stages {
        stage('Checkout') {
            steps {
                // Код скачивается автоматически из Git
                checkout scm
            }
        }

        stage('Docker Build') {
            steps {
                script {
                    // Jenkins запускает сборку образа, используя Dockerfile из корня (.)
                    // Все шаги (restore, build, test, publish) пройдут внутри Docker
                    customImage = docker.build("${IMAGE_NAME}:${TAG}", ".")
                }
            }
        }

        stage('Docker Push (Optional)') {
            steps {
                script {
                    // Если у вас есть Registry (Docker Hub/Nexus), пушим туда
                    echo "Pushing image ${IMAGE_NAME}:${TAG}..."
                    // docker.withRegistry('https://my-repo.com', 'credentials-id') {
                    //     customImage.push()
                    // }
                }
            }
        }
        
        stage('Deploy Local (Test)') {
            steps {
                // Можно сразу запустить свежий контейнер для проверки
                sh "docker rm -f bank-app-instance || true"
                sh "docker run -d --name bank-app-instance -p 5000:8080 ${IMAGE_NAME}:${TAG}"
            }
        }
    }

    post {
        always {
            // Очистка старых образов, чтобы не забивать место на сервере
            sh "docker image prune -f"
        }
    }
}
