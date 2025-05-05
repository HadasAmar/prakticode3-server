
# ToDo List Application

## Overview
This project is a Fullstack ToDo List application where users can register, log in, and manage their tasks. 

The application is divided into two parts:
- **Backend (Server)**: A Web API built using .NET with Entity Framework, hosted in Visual Studio.
- **Frontend (Client)**: A React-based client developed in Visual Studio Code.

### Features:
- **User Management**:
  - User Registration
  - User Login
- **Task Management**:
  - View all tasks of the logged-in user
  - Add new tasks
  - Update task status (Mark task as done)
  - Delete tasks
  
### Technologies Used:
- **Backend**: .NET, Entity Framework, MySQL
- **Frontend**: React
- **Development Environments**:
  - Backend: Visual Studio (VS)
  - Frontend: Visual Studio Code (VS Code)

---

## Setup Instructions

### Backend Setup:
1. **Install Required Packages**:
   Make sure you have the necessary NuGet packages installed in your project:
   - `Microsoft.EntityFrameworkCore`
   - `Microsoft.EntityFrameworkCore.MySql`
   - `Microsoft.EntityFrameworkCore.Tools`
   - `Microsoft.EntityFrameworkCore.Design`


2. **Database Setup**:
   - Create a MySQL database for the project.
   - Update the connection string in `appsettings.json` to reflect your database credentials.

3. **Run the Application**:
   You can run the backend API by opening it in Visual Studio and using the built-in tools to run the project.
   
4. **API Endpoints**:
   The server exposes several endpoints to manage users and tasks:
   - `POST /api/Customers/register` - Register a new user.
   - `POST /api/Customers/login` - Login to get a JWT token.
   - `GET /items/user/{id}` - Get all tasks for the logged-in user.
   - `POST /api/items` - Add a new task.
   - `PUT /api/items/{id}` - Update the task (mark it as completed).
   - `DELETE /api/items/{id}` - Delete a task.

---

### Frontend Setup:
1. **Install Dependencies**:
   From the frontend folder, run the following to install dependencies:
   ```bash
   npm install
   ```

2. **Run the Client**:
   Run the client using:
   ```bash
   npm start
   ```

   This will start the React application and connect to the backend API.

3. **Make sure the backend API is running** and accessible to the frontend.

---

## How It Works:
1. **User Registration**:
   - A user registers by providing their details (username and password).
   - Upon successful registration, the user can log in.

2. **User Login**:
   - After logging in, the user receives a JWT token which is used to authenticate subsequent requests.
   
3. **Task Management**:
   - Once logged in, the user can view their tasks.
   - Users can add, update (mark as completed), and delete tasks.
   
---

## Project Structure:

### Backend:
- `Controllers/` - Contains the API controllers for user and task management.
- `Models/` - Contains the entity classes, such as `Customer` and `Item`.
- `Data/` - Contains the DbContext.

### Frontend:
- `src/` - Contains the React components, including task management UI and service.

---




