using DataAccess;
using ExampleLibrary;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Models.Employee;
using Moq;
using System.Linq.Expressions;
using Xunit;
using static System.Threading.Tasks.Task;

namespace ExampleLibraryTests
{
    public class EmployeeTests
    {
        [Fact]
        public async Task GetEmployeesTask_ReturnsSortedEmployees()
        {
            var dateTime = DateTime.UtcNow;
            // Arrange
            var employees = new List<Employees>
            {
                new ()
                {
                    FirstName = "John",
                    LastName = "Zeta",
                    HireDate = dateTime
                },
                new ()
                {
                    FirstName = "Anna",
                    LastName = "Alpha",
                    HireDate = dateTime
                }
            }.AsQueryable();

            var mockSet = MockDbSet(employees);
            var mockContext = new Mock<IApplicationDbContext>();
            mockContext.Setup(c => c.Employees).Returns(mockSet.Object);

            var service = new Employee(mockContext.Object);

            // Act
            var result = await service.GetEmployeesTask();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal("Anna", result[0].FirstName);
        }

        [Fact]
        public async Task GetEmployeesTask_ReturnsEmptyList_WhenNoEmployeesExist()
        {
            // Arrange
            var emptyData = new List<Employees>().AsQueryable();
            var mockSet = MockDbSet(emptyData);

            var mockContext = new Mock<IApplicationDbContext>();
            mockContext.Setup(c => c.Employees).Returns(mockSet.Object);

            var service = new Employee(mockContext.Object);

            // Act
            var result = await service.GetEmployeesTask();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetEmployeesTask_ThrowsException_WhenDbFails()
        {
            // Arrange
            var mockSet = new Mock<DbSet<Employees>>();
            var mockContext = new Mock<IApplicationDbContext>();
            mockContext.Setup(c => c.Employees).Throws(new Exception("Database error"));

            var service = new Employee(mockContext.Object);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => service.GetEmployeesTask());
            Assert.Equal("Database error", ex.Message);
        }
        //[Fact()]
        //public void FilterEmployeesTaskTest()
        //{
        //    Xunit.Assert.Fail("This test needs an implementation");
        //}

        //[Fact()]
        //public void CreateEmployeeTest()
        //{
        //    Xunit.Assert.Fail("This test needs an implementation");
        //}

        //[Fact()]
        //public void EditEmployeeTest()
        //{
        //    Xunit.Assert.Fail("This test needs an implementation");
        //}

        //[Fact()]
        //public void DeleteEmployeeTest()
        //{
        //    Xunit.Assert.Fail("This test needs an implementation");
        //}

        // Helper to create async-compatible mocked DbSet
        private static Mock<DbSet<T>> MockDbSet<T>(IQueryable<T> data) where T : class
        {
            var mockSet = new Mock<DbSet<T>>();

            mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<T>(data.Provider));
            mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());

            mockSet.As<IAsyncEnumerable<T>>().Setup(d => d.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
                .Returns(new TestAsyncEnumerator<T>(data.GetEnumerator()));

            return mockSet;
        }
    }
}

    internal class TestAsyncQueryProvider<TEntity>(IQueryProvider inner) : IAsyncQueryProvider
    {
        public IQueryable CreateQuery(Expression expression) => new TestAsyncEnumerable<TEntity>(expression);
        public IQueryable<TElement> CreateQuery<TElement>(Expression expression) => new TestAsyncEnumerable<TElement>(expression);
        public object Execute(Expression expression) => inner.Execute(expression);
        public TResult Execute<TResult>(Expression expression) => inner.Execute<TResult>(expression);
        public IAsyncEnumerable<TResult> ExecuteAsync<TResult>(Expression expression) => new TestAsyncEnumerable<TResult>(expression);
        public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken)
        {
            return Execute<TResult>(expression);
        }
    }

    internal class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
    {
        public TestAsyncEnumerable(IEnumerable<T> enumerable) : base(enumerable) { }
        public TestAsyncEnumerable(Expression expression) : base(expression) { }

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
            => new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());

        IQueryProvider IQueryable.Provider => new TestAsyncQueryProvider<T>(this);
    }

    internal class TestAsyncEnumerator<T>(IEnumerator<T> inner) : IAsyncEnumerator<T>
    {
        public T Current => inner.Current;
        public ValueTask DisposeAsync() => new ValueTask();
        public ValueTask<bool> MoveNextAsync() => new ValueTask<bool>(inner.MoveNext());
    }