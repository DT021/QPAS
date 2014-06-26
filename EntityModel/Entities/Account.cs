﻿// -----------------------------------------------------------------------
// <copyright file="Account.cs" company="">
// Copyright 2014 Alexander Soffronow Pagonidis
// </copyright>
// -----------------------------------------------------------------------

using System.ComponentModel.DataAnnotations.Schema;

namespace EntityModel
{
    public class Account
    {
        public int ID { get; set; }

        [Index(IsUnique=true)]
        public string AccountId { get; set; }
    }
}
