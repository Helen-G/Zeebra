using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;

namespace AFT.RegoV2.Tests.Common.TestDoubles
{
    public class FakeFindableDbSet<T> : FakeDbSet<T> where T : class
    {
        private readonly Func<object[], ISet<T>, T> _finder;

        public FakeFindableDbSet(Func<object[], ISet<T>, T> finder)
        {
            _finder = finder;
        }

        public override T Find(params object[] keyValues)
        {
            return _finder(keyValues, Data);
        }
    }

    public class FakeDbSet<T> : IDbSet<T> where T : class
    {
        protected readonly HashSet<T> Data;
        private readonly IQueryable _query;

        public FakeDbSet()
        {
            Data = new HashSet<T>();
            _query = Data.AsQueryable();
        }

        public T Add(T entity)
        {
            lock(Data) Data.Add(entity);
            return entity;
        }

        public T Attach(T entity)
        {
            lock (Data) Data.Add(entity);
            return entity;
        }

        public TDerivedEntity Create<TDerivedEntity>() where TDerivedEntity : class, T
        {
            throw new NotImplementedException();
        }

        public void AddOrUpdate(Expression<Func<T, object>> identifierExpression, params T[] entities)
        {
            AddOrUpdate(entities);
        }

        public void AddOrUpdate(params T[] entities)
        {
            foreach (var entity in entities)
            {
                Add(entity);
            }
        }

        public T Create()
        {
            return Activator.CreateInstance<T>();
        }

        public virtual T Find(params object[] keyValues)
        {
            throw new NotImplementedException("Use FakeFindableDbSet");
        }

        public ObservableCollection<T> Local
        {
            get { return new ObservableCollection<T>(Data); }
        }

        public T Remove(T entity)
        {
            lock (Data) Data.Remove(entity);
            return entity;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return Data.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Data.GetEnumerator();
        }

        public Type ElementType
        {
            get { return _query.ElementType; }
        }

        public Expression Expression
        {
            get { return _query.Expression; }
        }

        public IQueryProvider Provider
        {
            get { return _query.Provider; }
        }

    }
}
