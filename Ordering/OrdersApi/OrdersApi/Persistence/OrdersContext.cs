﻿using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using OrdersApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrdersApi.Persistence
{
    public class OrdersContext:DbContext
    {

        public OrdersContext(DbContextOptions<OrdersContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            var converter = new EnumToStringConverter<Status>();
            builder
                .Entity<Order>()
                .Property(p => p.Status)
                .HasConversion(converter)
            ;
            builder.Entity<Order>()
                .Property(p => p.StatusString)
                .HasComputedColumnSql("[Status]",stored:false);
        }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
    }
}
