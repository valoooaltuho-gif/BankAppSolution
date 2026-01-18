pipeline {
    agent any

    environment {
        // Указываем полный путь к исполняемому файлу dotnet
        DOTNET_CLI = "/usr/bin/dotnet" 
    }

    stages {
        stage('Preparation') {
            steps {
                echo 'Checking tools...'
                // Используем переменную окружения
                sh "$DOTNET_CLI --version" 
            }
        }

        stage('Restore') {
            steps {
                sh "$DOTNET_CLI restore"
            }
        }

        stage('Build') {
            steps {
                sh "$DOTNET_CLI build --configuration Release --no-restore"
            }
        }

        stage('Run Tests') {
            steps {
                sh "$DOTNET_CLI test --no-build --configuration Release --logger 'junit;LogFilePath=test-results.xml'"
            }
            post {
                always {
                    junit '**/test-results.xml'
                }
            }
        }

        stage('Publish (Optional)') {
            steps {
                sh "$DOTNET_CLI publish -c Release -o ./publish"
            }
        }
    }
}
