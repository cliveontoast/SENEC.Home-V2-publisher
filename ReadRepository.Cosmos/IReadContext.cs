﻿using Microsoft.Azure.Cosmos;
using Repository.Model;
using System.Linq;

namespace ReadRepository.Cosmos
{
    public interface IReadContext
    {
        IOrderedQueryable<T> GetQueryable<T>() where T : IRepositoryEntity;
    }

    public class ReadContainer : IReadContext
    {
        private readonly Container _container;

        public ReadContainer(Container container)
        {
            _container = container;
        }
        public IOrderedQueryable<T> GetQueryable<T>() where T : IRepositoryEntity
        {
            return _container.GetItemLinqQueryable<T>();
        }
    }

}
