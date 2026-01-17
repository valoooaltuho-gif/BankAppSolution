pipeline {
    agent any // Запускать на любом свободном узле

    environment {
        // Указываем путь к проекту, если он не в корне
        DOTNET_CLI_HOME = "/tmp/dotnet_home"
    }

    stages {
        stage('Preparation') {
            steps {
                echo 'Checking tools...'
                sh 'dotnet --version' // Проверяем, что SDK доступен
            }
        }

        stage('Restore') {
            steps {
                sh 'dotnet restore'
            }
        }

        stage('Build') {
            steps {
                // Сборка без запуска тестов
                sh 'dotnet build --configuration Release --no-restore'
            }
        }

        stage('Test') {
            steps {
                echo 'Running Integration Tests...'
                // Запуск твоих тестов
                // --logger:junit создаст отчет, который Jenkins сможет прочитать
                sh 'dotnet test --no-build --configuration Release --logger "junit;LogFilePath=test-results.xml"'
            }
            post {
                always {
                    // Публикуем результаты тестов в интерфейс Jenkins
                    junit '**/test-results.xml'
                }
            }
        }

        stage('Publish (Optional)') {
            steps {
                // Создание готовой папки с приложением (артефакт)
                sh 'dotnet publish -c Release -o ./publish'
            }
        }
    }
}
